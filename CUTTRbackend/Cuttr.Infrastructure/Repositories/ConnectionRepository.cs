using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<ConnectionRepository> _logger;

        public ConnectionRepository(CuttrDbContext context, ILogger<ConnectionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Connection> GetConnectionByIdAsync(int connectionId)
        {
            _logger.LogInformation("Retrieving connection with ID {ConnectionId}.", connectionId);
            try
            {
                var efConnection = await _context.Connections
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);

                if (efConnection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    return null;
                }

                // Detach the entity to prevent tracking
                _context.Entry(efConnection).State = EntityState.Detached;

                var connection = EFToBusinessMapper.MapToConnection(efConnection);
                _logger.LogInformation("Successfully retrieved connection with ID {ConnectionId}.", connectionId);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving connection with ID {ConnectionId}.", connectionId);
                throw new RepositoryException("An error occurred while retrieving the connection.", ex);
            }
        }

        public async Task<IEnumerable<Connection>> GetConnectionsByUserIdAsync(int userId)
        {
            _logger.LogInformation("Retrieving connections for user with ID {UserId}.", userId);
            try
            {
                var efConnections = await _context.Connections
                    .Where(c => c.UserId1 == userId || c.UserId2 == userId)
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .ToListAsync();

                if (efConnections == null || !efConnections.Any())
                {
                    _logger.LogInformation("No connections found for user with ID {UserId}.", userId);
                    return Enumerable.Empty<Connection>();
                }

                // Detach entities to prevent tracking
                foreach (var efConnection in efConnections)
                {
                    _context.Entry(efConnection).State = EntityState.Detached;
                }

                var connections = efConnections.Select(EFToBusinessMapper.MapToConnection);
                _logger.LogInformation("Successfully retrieved {Count} connections for user with ID {UserId}.", connections.Count(), userId);
                return connections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving connections for user with ID {UserId}.", userId);
                throw new RepositoryException("An error occurred while retrieving connections.", ex);
            }
        }

        public async Task<Connection> CreateConnectionAsync(Connection connection)
        {
            _logger.LogInformation("Creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
            try
            {
                var efConnection = BusinessToEFMapper.MapToConnectionEF(connection);
                await _context.Connections.AddAsync(efConnection);
                await _context.SaveChangesAsync();

                // Detach the entity to prevent tracking
                _context.Entry(efConnection).State = EntityState.Detached;

                var createdConnection = EFToBusinessMapper.MapToConnection(efConnection);
                _logger.LogInformation("Successfully created connection with ID {ConnectionId}.", createdConnection.ConnectionId);
                return createdConnection;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database error occurred while creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
                throw new RepositoryException("A database error occurred while creating the connection.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
                throw new RepositoryException("An error occurred while creating the connection.", ex);
            }
        }

        public async Task<Connection> UpdateConnectionAsync(Connection connection)
        {
            _logger.LogInformation("Updating connection with ID {ConnectionId}.", connection.ConnectionId);
            try
            {
                var efConnection = BusinessToEFMapper.MapToConnectionEF(connection);

                _context.Connections.Update(efConnection);
                await _context.SaveChangesAsync();

                // Detach the entity to prevent tracking
                _context.Entry(efConnection).State = EntityState.Detached;

                var updatedConnection = EFToBusinessMapper.MapToConnection(efConnection);
                _logger.LogInformation("Successfully updated connection with ID {ConnectionId}.", connection.ConnectionId);
                return updatedConnection;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "A concurrency error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
                throw new RepositoryException("A concurrency error occurred while updating the connection.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
                throw new RepositoryException("A database error occurred while updating the connection.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
                throw new RepositoryException("An error occurred while updating the connection.", ex);
            }
        }

        public async Task<Connection> GetConnectionByUsersAsync(int userId1, int userId2)
        {
            _logger.LogInformation("Retrieving connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
            try
            {
                var efConnection = await _context.Connections
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .FirstOrDefaultAsync(c =>
                        (c.UserId1 == userId1 && c.UserId2 == userId2) ||
                        (c.UserId1 == userId2 && c.UserId2 == userId1));

                if (efConnection == null)
                {
                    _logger.LogWarning("Connection between User1 ID {UserId1} and User2 ID {UserId2} not found.", userId1, userId2);
                    return null;
                }

                // Detach the entity to prevent tracking
                _context.Entry(efConnection).State = EntityState.Detached;

                var connection = EFToBusinessMapper.MapToConnection(efConnection);
                _logger.LogInformation("Successfully retrieved connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
                throw new RepositoryException("An error occurred while retrieving the connection.", ex);
            }
        }

        public async Task<int> GetNumberOfMatchesAsync(int connectionId)
        {
            _logger.LogInformation("Retrieving number of matches for connection ID {ConnectionId}.", connectionId);
            try
            {
                var count = await _context.Matches
                    .Where(m => m.ConnectionId == connectionId)
                    .CountAsync();

                _logger.LogInformation("Connection ID {ConnectionId} has {Count} matches.", connectionId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving number of matches for connection ID {ConnectionId}.", connectionId);
                throw new RepositoryException("An error occurred while retrieving the number of matches.", ex);
            }
        }

    }
}
