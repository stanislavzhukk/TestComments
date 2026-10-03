namespace Application.DTO.Responses
{
    public sealed record CommentResponse(
        Guid Id,
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        Guid? ParentId,
        DateTime CreatedAt)
    {
        public List<CommentResponse> Replies { get; } = new();
    }
}