using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Cuttr.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CuttrDbContext _context;

        public RefreshTokenRepository(CuttrDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
        {
            var ef = new RefreshTokenEF
            {
                UserId = token.UserId,
                TokenHash = token.TokenHash,
                ExpiresAt = token.ExpiresAt,
                IsRevoked = token.IsRevoked,
                CreatedAt = token.CreatedAt,
                RevokedAt = token.RevokedAt
            };
            _context.RefreshTokens.Add(ef);
            await _context.SaveChangesAsync();
            token.RefreshTokenId = ef.RefreshTokenId;
            return token;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string tokenHash)
        {
            var ef = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (ef == null) return null;

            return new RefreshToken
            {
                RefreshTokenId = ef.RefreshTokenId,
                UserId = ef.UserId,
                TokenHash = ef.TokenHash,
                ExpiresAt = ef.ExpiresAt,
                IsRevoked = ef.IsRevoked,
                CreatedAt = ef.CreatedAt,
                RevokedAt = ef.RevokedAt
            };
        }

        public async Task RevokeRefreshTokenAsync(string tokenHash)
        {
            var ef = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
            if (ef != null && !ef.IsRevoked)
            {
                ef.IsRevoked = true;
                ef.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRefreshTokensForUserAsync(int userId)
        {
            var tokens = _context.RefreshTokens.Where(t => t.UserId == userId);
            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}
