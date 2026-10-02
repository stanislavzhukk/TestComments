using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public required Guid CommentId { get; set; }
        public required Comment Comment { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public Guid InternalFileName { get; set; } = Guid.CreateVersion7();
        public string FilePath { get; set; } = string.Empty;
        public AttachmentType Type { get; set; }
    }
}
