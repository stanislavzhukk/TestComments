using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Application.Extensions;
using Application.DTO.Requests.Comments.Create;
using Application.DTO.Requests.Comments.Get;

namespace Application.Services
{
    public class CommentService(IAppDbContext appDbContext, ICommentContentSanitizer messageSanitizer, 
        ICaptchaService captchaService, IValidator<CreateCommentRequest> validator, IFileStorageService fileStorage) : ICommentService
    {
        public async Task<Result<CommentResponse>> CreateCommentAsync(CreateCommentRequest request, ClientInfoRequest client, CancellationToken ct = default)
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Result.Failure<CommentResponse>(validation.ToError());

            var captchaResult = captchaService.ValidateCaptcha(request.Captcha);

            if (!captchaResult.IsSuccess)
            {
                return Result.Failure<CommentResponse>(captchaResult.Error!);
            }

            var sanitizedHtml = messageSanitizer.Sanitize(request.Content);
            if (!sanitizedHtml.IsSuccess)
            {
                return Result.Failure<CommentResponse>(sanitizedHtml.Error!);
            }

            Guid? rootId = null;
            if (request.ParentCommentId is not null)
            {
                var parent = await appDbContext.Comments.AsNoTracking()
                    .Where(c => c.Id == request.ParentCommentId)
                    .Select(c => new { c.Id, c.RootId })
                    .FirstOrDefaultAsync(ct);

                if (parent is null)
                {
                    return Result.Failure<CommentResponse>(Error.NotFound("Comment.ParentNotFound", "Parent comment not found"));
                }
                rootId = parent.RootId ?? parent.Id;
            }

            Attachment? attachment = null;
            if (request.File is not null)
            {
                var saved = await fileStorage.SaveAsync(request.File, ct);
                if (saved.IsFailure)
                {
                    return Result.Failure<CommentResponse>(saved.Error!);
                }

                StoredFileResponse file = saved.Value!;
                attachment = new Attachment
                {
                    Type = file.Type,
                    StoredFileName = file.StoredFileName,
                    OriginalFileName = SafeName(request.File.FileName),
                };
            }

            var comment = new Comment
            {
                UserName = request.UserName,
                UserEmail = request.UserEmail,
                HomePageUrl = request.HomePageUrl,
                Content = sanitizedHtml.Value!,
                ParentId = request.ParentCommentId,
                RootId = rootId,
                UserIp = client.Ip,
                UserAgent = client.UserAgent,
                CreatedAt = DateTime.UtcNow
            };
            if (attachment is not null)
            {
                comment.Attachments.Add(attachment);
            }

            appDbContext.Comments.Add(comment);

            try
            {
                await appDbContext.SaveChangesAsync(ct);
            }
            catch
            {
                if (attachment is not null)
                {
                    fileStorage.Delete(attachment.StoredFileName);
                }
                throw;
            }

            var response = new CommentResponse(
                comment.Id,
                comment.UserName,
                comment.UserEmail,
                comment.HomePageUrl,
                comment.Content,
                comment.ParentId,
                comment.CreatedAt
            );

            if (attachment is not null)
            {
                response.Attachments.Add(new AttachmentResponse(
                    attachment.Type, 
                    $"/uploads/{attachment.StoredFileName}", 
                    attachment.OriginalFileName));
            }
            return Result.Success(response);
        }

        public async Task<Result<PagedResult<CommentResponse>>> GetCommentsAsync(GetPagedCommentsRequest request, CancellationToken ct = default)
        {
            var pageSize = Math.Clamp(request.PageSize, 1, 25);
            var pageNumber = Math.Max(request.PageNumber, 1);
            var rootCommentsQuery = appDbContext.Comments.AsNoTracking()
                .Where(c => c.ParentId == null);

            var totalCount = await rootCommentsQuery.CountAsync(ct);

            var sortedQuery = ApplySorting(rootCommentsQuery, request.SortBy, request.Desc);

            var rootComments = await sortedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Attachments)
                .ToListAsync(ct);

            var responses = rootComments.Select(c => MapToCommentResponse(c)).ToList();

            await AttachRepliesAsync(responses, ct);

            return Result.Success(new PagedResult<CommentResponse>(responses, totalCount, pageNumber, pageSize));
        }

        public async Task<Result<CommentResponse>> GetCommentAsync(Guid id, CancellationToken ct = default)
        {
            var comment = await appDbContext.Comments.AsNoTracking()
                .Include(c => c.Attachments)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync(ct);

            if (comment is null)
            {
                return Result.Failure<CommentResponse>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }

            var response = MapToCommentResponse(comment);
            return Result.Success(response);
        }

        private async Task AttachRepliesAsync(List<CommentResponse> roots, CancellationToken ct)
        {
            if (roots.Count == 0)
            {
                return;
            }

            var rootIds = roots.Select(r => r.Id).ToList();

            var replies = await appDbContext.Comments.AsNoTracking()
                .Where(c => c.RootId != null && rootIds.Contains(c.RootId.Value))
                .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
                .Include(c => c.Attachments)
                .ToListAsync(ct);

            var repliesResponses = replies.Select(c => MapToCommentResponse(c)).ToList();

            Dictionary<Guid, CommentResponse> commentsById = roots.Concat(repliesResponses).ToDictionary(x => x.Id);

            foreach (var reply in repliesResponses)
            {
                commentsById[reply.ParentId!.Value].Replies.Add(reply);
            }

            var replyCounts = await appDbContext.Comments.AsNoTracking()
                .Where(c => c.RootId != null && rootIds.Contains(c.RootId.Value))
                .GroupBy(c => c.RootId!.Value)
                .Select(group => new { RootId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.RootId, x => x.Count, ct);

            foreach (var root in roots)
            {
                root.SetRepliesCount(replyCounts.GetValueOrDefault(root.Id));
            }
        }

        private static IQueryable<Comment> ApplySorting(IQueryable<Comment> query, string? sortBy, bool desc)
        {
            return (sortBy, desc) switch
            {
                ("userName", true) => query.OrderByDescending(c => c.UserName).ThenByDescending(c => c.Id),
                ("userName", false) => query.OrderBy(c => c.UserName).ThenBy(c => c.Id),
                ("userEmail", true) => query.OrderByDescending(c => c.UserEmail).ThenByDescending(c => c.Id),
                ("userEmail", false) => query.OrderBy(c => c.UserEmail).ThenBy(c => c.Id),
                ("createdAt", false) => query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id),
                _ => query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id)
            };
        }

        private static string SafeName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            return name.Length > 255 ? name[..255] : name;
        }

        private static CommentResponse MapToCommentResponse(Comment comment)
        {
            var response = new CommentResponse(
                comment.Id,
                comment.UserName,
                comment.UserEmail,
                comment.HomePageUrl,
                comment.Content,
                comment.ParentId,
                comment.CreatedAt);

            foreach (var attachment in comment.Attachments)
            {
                response.Attachments.Add(new AttachmentResponse(
                    attachment.Type,
                    $"/uploads/{attachment.StoredFileName}",
                    attachment.OriginalFileName));
            }
            return response;
        }
    }
}
