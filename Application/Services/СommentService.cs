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

            rootCommentsQuery = ApplySorting(rootCommentsQuery, request.SortBy, request.Desc);

            var rootComments = await rootCommentsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentResponse(
                    c.Id,
                    c.UserName,
                    c.UserEmail,
                    c.HomePageUrl,
                    c.Content,
                    c.ParentId,
                    c.CreatedAt
                )).ToListAsync(ct);

            var rootIds = rootComments.Select(r => r.Id).ToList();

            await AttachRepliesAsync(rootComments, ct);

            return Result.Success(new PagedResult<CommentResponse>(rootComments, totalCount, pageNumber, pageSize));
        }

        public async Task<Result<CommentResponse>> GetCommentAsync(Guid id, CancellationToken ct = default)
        {
            var comment = await appDbContext.Comments.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CommentResponse(
                    c.Id,
                    c.UserName,
                    c.UserEmail,
                    c.HomePageUrl,
                    c.Content,
                    c.ParentId,
                    c.CreatedAt
                ))
                .FirstOrDefaultAsync(ct);
            if (comment is null)
            {
                return Result.Failure<CommentResponse>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }
            return Result.Success(comment);
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
                .Select(c => new CommentResponse(
                    c.Id, 
                    c.UserName, 
                    c.UserEmail, 
                    c.HomePageUrl,
                    c.Content, 
                    c.ParentId, 
                    c.CreatedAt))
                .ToListAsync(ct);

            await AttachFilesAsync([.. roots, .. replies], ct);

            Dictionary<Guid, CommentResponse> commentsById = roots.Concat(replies).ToDictionary(x => x.Id);

            foreach (var reply in replies)
            {
                commentsById[reply.ParentId!.Value].Replies.Add(reply);
            }
        }

        private async Task AttachFilesAsync(IReadOnlyCollection<CommentResponse> comments, CancellationToken ct)
        {
            var ids = comments.Select(c => c.Id).ToList();

            var attachments = await appDbContext.Attachments.AsNoTracking()
                .Where(a => ids.Contains(a.CommentId))
                .Select(a => new { a.CommentId, a.Type, a.StoredFileName, a.OriginalFileName })
                .ToListAsync(ct);

            var commentsById = comments.ToDictionary(c => c.Id);

            foreach (var a in attachments)
            {
                commentsById[a.CommentId].Attachments.Add(
                    new AttachmentResponse(a.Type, $"/uploads/{a.StoredFileName}", a.OriginalFileName));
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
    }
}
