using Domain.Common;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Comment : BaseEntity
    {
        public Guid? RootId { get; set; }
        public Guid? ParentId { get; set; }
        [MaxLength(CommentLimits.UserNameMax)]
        public required string UserName { get; set; }
        [MaxLength(CommentLimits.EmailMax)]
        public required string UserEmail { get; set; }
        [MaxLength(CommentLimits.ContentMax)]
        public required string Content { get; set; }
        [MaxLength(CommentLimits.HomePageMax)]
        public string? HomePageUrl { get; set; }
        public string? UserIp { get; set; }
        public string? UserAgent { get; set; }
        public required DateTime CreatedAt { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
