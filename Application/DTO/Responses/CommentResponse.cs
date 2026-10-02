namespace Application.DTO.Responses
{
    public sealed record CommentResponse(
        Guid Id,
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        Guid? ParentCommentId
    );
}
