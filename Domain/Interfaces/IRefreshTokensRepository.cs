using Domain.Models;

namespace Domain.Interfaces
{
    public interface IRefreshTokensRepository
    {
        Task AddRefreshTokenAsync(RefreshToken token);
        Task DeleteExpiredAndRevokedAsync();
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(RefreshToken tokenEntity);
        Task RevokeTokenAsync(RefreshToken tokenEntity);
    }
}
