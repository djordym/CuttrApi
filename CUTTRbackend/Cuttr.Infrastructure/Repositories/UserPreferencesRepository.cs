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
    public class UserPreferencesRepository : IUserPreferencesRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<UserPreferencesRepository> _logger;

        public UserPreferencesRepository(CuttrDbContext context, ILogger<UserPreferencesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserPreferences> GetUserPreferencesAsync(int userId)
        {
            try
            {
                var efPreferences = await _context.UserPreferences.AsNoTracking()
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                return EFToBusinessMapper.MapToUserPreferences(efPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user preferences for user ID {userId}.");
                throw new RepositoryException("An error occurred while retrieving user preferences.", ex);
            }
        }

        public async Task<UserPreferences> AddUserPreferencesAsync(UserPreferences preferences)
        {
            try
            {
                var efPreferences = BusinessToEFMapper.MapToUserPreferencesEF(preferences);

                await _context.UserPreferences.AddAsync(efPreferences);
                await _context.SaveChangesAsync();
                _context.Entry(efPreferences).State = EntityState.Detached;
                return EFToBusinessMapper.MapToUserPreferences(efPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user preferences.");
                throw new RepositoryException("An error occurred while adding user preferences.", ex);
            }
        }

        public async Task UpdateUserPreferencesAsync(UserPreferences preferences)
        {
            try
            {
                var efPreferences = BusinessToEFMapper.MapToUserPreferencesEF(preferences);
                _context.UserPreferences.Update(efPreferences);
                await _context.SaveChangesAsync();
                _context.Entry(efPreferences).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user preferences for user ID {preferences.UserId}.");
                throw new RepositoryException("An error occurred while updating user preferences.", ex);
            }
        }
    }
}
