using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RefreshTokensRepository : IRefreshTokensRepository
    {
        private readonly ApplicationDbContext _context;
        public RefreshTokensRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            await _context.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);
            return tokenEntity;
        }

        public async Task UpdateAsync(RefreshToken tokenEntity)
        {
            _context.RefreshTokens.Update(tokenEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredAndRevokedAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.Expires <= DateTime.UtcNow || t.Revoked != null)
                .ToListAsync();
            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }

        public Task RevokeTokenAsync(RefreshToken tokenEntity)
        {
            tokenEntity.Revoked = DateTime.UtcNow;
            _context.RefreshTokens.Update(tokenEntity);
            return _context.SaveChangesAsync();
        }
    }
}
