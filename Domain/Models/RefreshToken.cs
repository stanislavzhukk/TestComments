
namespace Domain.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }

        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
    }
}