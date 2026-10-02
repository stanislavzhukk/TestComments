using Application.DTO.Requests.Comment;
using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CommentService(IAppDbContext appDbContext) : ICommentService
    {
        public async Task<Result<CommentResponse>> CreateCommentAsync(CreateCommentRequest request, CancellationToken ct = default)
        {
            Guid? rootId = null;
            if (request.ParentCommentId is not null)
            {
                var parent = await appDbContext.Comments.AsNoTracking()
                    .Where(c => c.Id == request.ParentCommentId)
                    .Select(c => new { c.Id, c.RootId })
                    .FirstOrDefaultAsync(ct);

                if (parent is null)
                    return Result.Failure<CommentResponse>(Error.NotFound("Comment.ParentNotFound", "Parent comment not found"));

                rootId = parent.RootId ?? parent.Id;
            }

            //captcha validation and other business logic

            //html sanitization and validation

            var html = request.Content; //sanitize html

            //parentid and rootid validation

            //file upload 
            Attachment? attachment = null; //upload file and get attachment entity

            //create comment entity and save to db



            var comment = new Comment
            {
                UserName = request.UserName,
                UserEmail = request.UserEmail,
                HomePageUrl = request.HomePageUrl,
                Content = html,
                ParentId = request.ParentCommentId,
                //AuthorIp = client.Ip,
                //AuthorUserAgent = client.UserAgent,
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
                    //files.Delete(attachment.StoredFileName);
                }
                throw;                                         
            }

            return Result.Success(new CommentResponse(
                comment.Id,
                comment.UserName,
                comment.UserEmail,
                comment.HomePageUrl,
                comment.Content,
                comment.ParentId
            ));
        }

        public async Task<Result<PagedResult<CommentResponse>>> GetCommentsAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var comments = await appDbContext.Comments.AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentResponse(
                    c.Id,
                    c.UserName,
                    c.UserEmail,
                    c.HomePageUrl,
                    c.Content,
                    c.ParentId
                ))
                .ToListAsync();
            return Result.Success(new PagedResult<CommentResponse>(comments, comments.Count, page, pageSize));
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
                    c.ParentId
                ))
                .FirstOrDefaultAsync(ct);
            if (comment is null)
            {
                return Result.Failure<CommentResponse>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }
            return Result.Success(comment);
        }
    }
}
