using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class User : IdentityUser<Guid>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdate { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}