using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<MatchRepository> _logger;

        public MatchRepository(CuttrDbContext context, ILogger<MatchRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Match>> GetMatchesByConnectionIdAsync(int connectionId)
        {
            try
            {
                var efMatches = await _context.Matches
                    .AsNoTracking()
                    .Include(m => m.Plant1)
                    .Include(m => m.Plant2)
                    .Where(m => m.ConnectionId == connectionId)
                    .ToListAsync();

                return efMatches.Select(EFToBusinessMapper.MapToMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving matches for connection with ID {connectionId}.");
                throw new RepositoryException("An error occurred while retrieving matches.", ex);
            }
        }

        public async Task<Match> GetMatchByIdAsync(int matchId)
        {
            try
            {
                var efMatch = await _context.Matches
                    .AsNoTracking()
                    .Include(m => m.Plant1)
                    .Include(m => m.Plant2)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                return EFToBusinessMapper.MapToMatch(efMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving match with ID {matchId}.");
                throw new RepositoryException("An error occurred while retrieving match.", ex);
            }
        }
        public async Task<Match> AddMatchAsync(Match match)
        {
            try
            {
                var efMatch = BusinessToEFMapper.MapToMatchEF(match);

                await _context.Matches.AddAsync(efMatch);
                await _context.SaveChangesAsync();
                _context.Entry(efMatch).State = EntityState.Detached;
                
                return EFToBusinessMapper.MapToMatch(efMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a match.");
                throw new RepositoryException("An error occurred while adding a match.", ex);
            }
        }

    }
}
