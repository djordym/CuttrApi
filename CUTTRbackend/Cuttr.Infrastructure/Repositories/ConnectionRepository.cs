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
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<ConnectionRepository> _logger;

        //public ConnectionRepository(CuttrDbContext context, ILogger<ConnectionRepository> logger)
        //{
        //    _context = context;
        //    _logger = logger;
        //}

        //public async Task<IEnumerable<Connection>> GetMatchesByUserIdAsync(int userId)
        //{
        //    try
        //    {
        //        var efMatches = await _context.Connections
        //            .AsNoTracking()
        //            .Include(m => m.Plant1)
        //            .Include(m => m.Plant2)
        //            .Include(m => m.User1)
        //            .Include(m => m.User2)
        //            .Where(m => m.UserId1 == userId || m.UserId2 == userId)
        //            .ToListAsync();

        //        return efMatches.Select(EFToBusinessMapper.MapToMatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"An error occurred while retrieving matches for user with ID {userId}.");
        //        throw new RepositoryException("An error occurred while retrieving matches.", ex);
        //    }
        //}

        //public async Task<Connection> GetMatchByIdAsync(int matchId)
        //{
        //    try
        //    {
        //        var efMatch = await _context.Connections
        //            .AsNoTracking()
        //            .Include(m => m.Plant1)
        //            .Include(m => m.Plant2)
        //            .Include(m => m.User1)
        //            .Include(m => m.User2)
        //            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        //        return EFToBusinessMapper.MapToMatch(efMatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"An error occurred while retrieving match with ID {matchId}.");
        //        throw new RepositoryException("An error occurred while retrieving match.", ex);
        //    }
        //}
        //public async Task<Connection> AddMatchAsync(Connection match)
        //{
        //    try
        //    {
        //        var efMatch = BusinessToEFMapper.MapToMatchEF(match);

        //        await _context.Connections.AddAsync(efMatch);
        //        await _context.SaveChangesAsync();
        //        _context.Entry(efMatch).State = EntityState.Detached;

        //        return EFToBusinessMapper.MapToMatch(efMatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while adding a match.");
        //        throw new RepositoryException("An error occurred while adding a match.", ex);
        //    }
        //}

        public async Task<Connection> GetConnectionByIdAsync(int connectionId)
        {
            // Load the EF entity (including navigation to the two users if needed)
            var ef = await _context.Connections
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);

            if (ef == null) return null;
            _context.Entry(ef).State = EntityState.Detached;
            // Convert EF entity to domain model
            return EFToBusinessMapper.MapToConnection(ef);
        }

        public async Task<IEnumerable<Connection>> GetConnectionsByUserIdAsync(int userId)
        {
            var efConnections = await _context.Connections
                .Where(c => c.UserId1 == userId || c.UserId2 == userId)
                .Include(c => c.User1)
                .Include(c => c.User2)
                .ToListAsync();
            _context.Entry(efConnections).State = EntityState.Detached;
            return efConnections.Select(EFToBusinessMapper.MapToConnection);
        }

        public async Task<Connection> CreateConnectionAsync(Connection connection)
        {
            // Convert domain model -> EF entity
            var ef = BusinessToEFMapper.MapToConnectionEF(connection);

            // EF insert
            _context.Connections.Add(ef);
            await _context.SaveChangesAsync();
            _context.Entry(ef).State = EntityState.Detached;

            // After saving, ef.ConnectionId is populated
            return EFToBusinessMapper.MapToConnection(ef);
        }

        public async Task<Connection> UpdateConnectionAsync(Connection connection)
        {
            // Usually you'd load the existing EF entity from DB, then update fields.
            // For brevity, here's a direct approach if you trust the domain object.
            var ef = BusinessToEFMapper.MapToConnectionEF(connection);

            // EF update
            _context.Connections.Update(ef);
            await _context.SaveChangesAsync();
            _context.Entry(ef).State = EntityState.Detached;

            return EFToBusinessMapper.MapToConnection(ef);
        }

        public async Task<Connection> GetConnectionByUsersAsync(int userId1, int userId2)
        {
            var efConnection = await _context.Connections
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => (c.UserId1 == userId1 && c.UserId2 == userId2) || (c.UserId1 == userId2 && c.UserId2 == userId1));

            _context.Entry(efConnection).State = EntityState.Detached;

            return EFToBusinessMapper.MapToConnection(efConnection);
        }
    }
}


