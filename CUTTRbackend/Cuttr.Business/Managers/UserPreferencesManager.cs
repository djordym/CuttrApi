using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class UserPreferencesManager : IUserPreferencesManager
    {
        private readonly IUserPreferencesRepository _userPreferencesRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserPreferencesManager> _logger;

        public UserPreferencesManager(
            IUserPreferencesRepository userPreferencesRepository,
            IUserRepository userRepository,
            ILogger<UserPreferencesManager> logger)
        {
            _userPreferencesRepository = userPreferencesRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserPreferencesResponse> GetUserPreferencesAsync(int userId)
        {
            try
            {
                var preferences = await _userPreferencesRepository.GetUserPreferencesAsync(userId);
                if (preferences == null)
                {
                    throw new NotFoundException($"User preferences for user ID {userId} not found.");
                }

                return BusinessToContractMapper.MapToUserPreferencesResponse(preferences);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user preferences for user ID {userId}.");
                throw new BusinessException("Error retrieving user preferences.", ex);
            }
        }

        public async Task<UserPreferencesResponse> CreateOrUpdateUserPreferencesAsync(int userId, UserPreferencesRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                var preferences = await _userPreferencesRepository.GetUserPreferencesAsync(userId);

                if (preferences == null)
                {
                    // Create new preferences
                    preferences = ContractToBusinessMapper.MapToUserPreferences(request);
                    preferences.UserId = userId;

                    var createdPreferences = await _userPreferencesRepository.AddUserPreferencesAsync(preferences);
                    return BusinessToContractMapper.MapToUserPreferencesResponse(createdPreferences);
                }
                else
                {
                    // Update existing preferences
                    ContractToBusinessMapper.MapToUserPreferences(request, preferences);
                    await _userPreferencesRepository.UpdateUserPreferencesAsync(preferences);

                    return BusinessToContractMapper.MapToUserPreferencesResponse(preferences);
                }
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating or updating user preferences for user ID {userId}.");
                throw new BusinessException("Error creating or updating user preferences.", ex);
            }
        }
    }
}
