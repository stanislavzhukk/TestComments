using Application.DTO.Requests.Comment;
using Application.DTO.Responses;
using Domain.Common;

namespace Application.Interfaces
{
    public interface ICommentService
    {
        Task<Result<PagedResult<CommentResponse>>> GetCommentsAsync(GetPagedCommentsRequest request, CancellationToken ct = default);
        Task<Result<CommentResponse>> GetCommentAsync(Guid id, CancellationToken ct = default);
        Task<Result<CommentResponse>> CreateCommentAsync(CreateCommentRequest createCommentDto, CancellationToken ct = default);
    }
}
