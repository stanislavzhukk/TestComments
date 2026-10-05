using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
        public required string OriginalFileName { get; set; }
        public required string StoredFileName { get; set; }
        public AttachmentType Type { get; set; }
    }
}
