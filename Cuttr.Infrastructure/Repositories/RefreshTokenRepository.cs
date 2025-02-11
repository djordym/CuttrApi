using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Entities;
using Cuttr.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<RefreshTokenRepository> _logger;

        public RefreshTokenRepository(CuttrDbContext context, ILogger<RefreshTokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
        {
            try
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
                _context.RefreshTokens.AddAsync(ef);
                await _context.SaveChangesAsync();
                _context.Entry(ef).State = EntityState.Detached;
                token.RefreshTokenId = ef.RefreshTokenId;
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a refresh token for user with ID {UserId}.", token.UserId);
                throw new RepositoryException("An error occurred while creating a refresh token.", ex);
            }
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string tokenHash)
        {
            try
            {
                var ef = await _context.RefreshTokens.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

                if (ef == null)
                {
                    _logger.LogWarning("No valid refresh token found for the provided token hash.");
                    return null;
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a refresh token by hash.");
                throw new RepositoryException("An error occurred while retrieving a refresh token.", ex);
            }
        }

        public async Task RevokeRefreshTokenAsync(string tokenHash)
        {
            try
            {
                var ef = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
                if (ef != null && !ef.IsRevoked)
                {
                    ef.IsRevoked = true;
                    ef.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _context.Entry(ef).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while revoking the refresh token for token hash: {TokenHash}", tokenHash);
                throw new RepositoryException("An error occurred while revoking the refresh token.", ex);
            }
        }

        public async Task DeleteRefreshTokensForUserAsync(int userId)
        {
            try
            {
                var tokens = _context.RefreshTokens.Where(t => t.UserId == userId);
                _context.RefreshTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting refresh tokens for user with ID {UserId}.", userId);
                throw new RepositoryException("An error occurred while deleting refresh tokens.", ex);
            }
        }
    }
}
