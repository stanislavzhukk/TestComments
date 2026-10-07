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
        public List<AttachmentResponse> Attachments { get; } = new();
        public int RepliesCount { get; private set; }

        public void SetRepliesCount(int count) => RepliesCount = count;
    }
}