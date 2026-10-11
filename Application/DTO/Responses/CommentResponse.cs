namespace Application.DTO.Responses
{
    public sealed record CommentResponse(
        Guid Id,
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        Guid? ParentId,
        DateTime CreatedAt,
        List<AttachmentResponse> Attachments)
    {
        public List<CommentResponse> Replies { get; private set; } = new();
        public int RepliesCount { get; private set; }

        public void SetRepliesCount(int count) => RepliesCount = count;
    }
}