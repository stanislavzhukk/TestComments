using Domain.Common;

namespace Domain.Entities
{
    public class Comment : BaseEntity
    {
        public Guid? RootId { get; set; }
        public Guid? ParentId { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public required string Content { get; set; }
        public string? HomePageUrl { get; set; }
        //todo x-user-id header
        public required DateTime CreatedAt { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
