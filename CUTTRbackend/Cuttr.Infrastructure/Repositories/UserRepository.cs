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
    public class UserRepository : IUserRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(CuttrDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                var efUser = BusinessToEFMapper.MapToUserEF(user);
                efUser.UserId = 0; // Ensure the ID is unset for new entities

                await _context.Users.AddAsync(efUser);
                await _context.SaveChangesAsync();

                // Map back to business entity
                return EFToBusinessMapper.MapToUser(efUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a user.");
                throw new RepositoryException("An error occurred while creating a user.", ex);
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var efUser = await _context.Users
                    .Include(u => u.Plants)
                    .Include(u => u.Preferences)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (efUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return null;
                }

                return EFToBusinessMapper.MapToUser(efUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with ID {userId}.");
                throw new RepositoryException("An error occurred while retrieving user.", ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var efUser = await _context.Users
                    .Include(u => u.Plants)
                    .Include(u => u.Preferences)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (efUser == null)
                {
                    _logger.LogWarning($"User with email {email} not found.");
                    return null;
                }

                return EFToBusinessMapper.MapToUser(efUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with email {email}.");
                throw new RepositoryException("An error occurred while retrieving user.", ex);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                var efUser = BusinessToEFMapper.MapToUserEF(user);

                _context.Users.Update(efUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"A concurrency error occurred while updating user with ID {user.UserId}.");
                throw new RepositoryException("A concurrency error occurred while updating user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with ID {user.UserId}.");
                throw new RepositoryException("An error occurred while updating user.", ex);
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var efUser = await _context.Users.FindAsync(userId);
                if (efUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return;
                }

                _context.Users.Remove(efUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"A database error occurred while deleting user with ID {userId}.");
                throw new RepositoryException("A database error occurred while deleting user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with ID {userId}.");
                throw new RepositoryException("An error occurred while deleting user.", ex);
            }
        }
    }
}
