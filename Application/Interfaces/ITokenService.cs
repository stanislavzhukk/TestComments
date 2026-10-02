
using Domain.Models;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<RefreshToken> GenerateRefreshTokenAsync(User user);
    }
}
