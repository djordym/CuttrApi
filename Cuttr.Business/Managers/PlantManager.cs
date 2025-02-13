using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Interfaces.Services;
using Cuttr.Business.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class PlantManager : IPlantManager
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISwipeRepository _swipeRepository;
        private readonly ILogger<PlantManager> _logger;
        private readonly IBlobStorageService _blobStorageService;

        private const string PlantImagesContainer = "plant-images";

        public PlantManager(IPlantRepository plantRepository, IUserRepository userRepository, ISwipeRepository swipeRepository, ILogger<PlantManager> logger, IBlobStorageService blobStorageService)
        {
            _plantRepository = plantRepository;
            _userRepository = userRepository;
            _swipeRepository = swipeRepository;
            _logger = logger;
            _blobStorageService = blobStorageService;
        }

        public async Task<PlantResponse> AddPlantAsync(PlantCreateRequest request, int userId)
        {
            try
            {
                // Validate that the user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                string imageUrl = null;

                if (request.Image != null && request.Image.Length > 0)
                {
                    // Upload image to Azure Blob Storage in 'plant-images' container
                    imageUrl = await _blobStorageService.UploadFileAsync(request.Image, PlantImagesContainer);
                }

                var plant = ContractToBusinessMapper.MapToPlant(request.PlantDetails);
                plant.ImageUrl = imageUrl;
                plant.UserId = userId;
                plant.IsTraded = false;

                var createdPlant = await _plantRepository.AddPlantAsync(plant);

                return BusinessToContractMapper.MapToPlantResponse(createdPlant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding plant.");
                throw new BusinessException("Error adding plant.", ex);
            }
        }

        public async Task<PlantResponse> GetPlantByIdAsync(int plantId)
        {
            try
            {
                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }
                return BusinessToContractMapper.MapToPlantResponse(plant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plant with ID {plantId}.");
                throw new BusinessException("Error retrieving plant.", ex);
            }
        }

        public async Task<PlantResponse> UpdatePlantAsync(int plantId, int userId, PlantRequest request)
        {
            try
            {

                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }

                // check if plant belong to user
                if (plant.UserId != userId)
                {
                    throw new BusinessException("Plant does not belong to user.");
                }

                // Update plant properties
                ContractToBusinessMapper.MapToPlantForUpdate(request, plant);

                await _plantRepository.UpdatePlantAsync(plant);

                return BusinessToContractMapper.MapToPlantResponse(plant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating plant with ID {plantId}.");
                throw new BusinessException("Error updating plant.", ex);
            }
        }

        public async Task DeletePlantAsync(int plantId, int userId)
        {
            try
            {
                //check if plant belongs to user

                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }

                await _plantRepository.DeletePlantAsync(plantId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting plant with ID {plantId}.");
                throw new BusinessException("Error deleting plant.", ex);
            }
        }

        public async Task<List<PlantResponse>> GetPlantsByUserIdAsync(int userId)
        {
            try
            {
                // Validate that the user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                var plants = await _plantRepository.GetTradablePlantsByUserIdAsync(userId);

                return BusinessToContractMapper.MapToPlantResponse(plants).ToList();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
                throw new BusinessException("Error retrieving plants.", ex);
            }
        }

        public async Task<List<PlantResponse>> GetLikablePlantsAsync(int userId, int maxCount)
        {
            // 1. Get the user with its preferences
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new BusinessException("User not found.");
            if (user.Preferences == null)
                throw new BusinessException("User preferences not found.");
            if (user.LocationLatitude == null || user.LocationLongitude == null)
                throw new BusinessException("User location not set.");

            // 2. Set search radius (fallback to 10km if not set)
            int radiusKm = user.Preferences.SearchRadius > 0 ? user.Preferences.SearchRadius : 10;

            // 3. Retrieve candidate plants already filtered by location, preferences, not owned by the user,
            // and in randomized order.
            var candidatePlants = await _plantRepository.GetFilteredTradablePlantsAsync(
                                        userId,
                                        user.LocationLatitude.Value,
                                        user.LocationLongitude.Value,
                                        radiusKm,
                                        user.Preferences
                                    );

            if (!candidatePlants.Any())
                return new List<PlantResponse>();

            // 4. Retrieve all of the user's plants (their IDs) to check swipe interactions.
            var userPlants = await _plantRepository.GetTradablePlantsByUserIdAsync(userId);
            var userPlantIds = userPlants.Select(up => up.PlantId).ToList();

            // 5. Bulk-load swipe data for candidate plants against the user's plants.
            var candidatePlantIds = candidatePlants.Select(p => p.PlantId).ToList();
            var swipeData = await _swipeRepository.GetSwipesForUserPlantsAsync(userPlantIds, candidatePlantIds);
            // Group swipes by candidate plant id
            var swipeCounts = swipeData.GroupBy(s => s.SwipedPlantId)
                                       .ToDictionary(g => g.Key, g => g.Count());

            // 6. Select only those candidate plants that have at least one of the user's plants
            // that hasn’t yet swiped (i.e. not all user plants have swiped on it).
            var likablePlants = new List<PlantResponse>();
            foreach (var plant in candidatePlants)
            {
                // If there are no swipes or not all of the user's plants have swiped...
                if (!swipeCounts.TryGetValue(plant.PlantId, out int swipeCount) || swipeCount < userPlantIds.Count)
                {
                    likablePlants.Add(BusinessToContractMapper.MapToPlantResponse(plant));
                }
                if (likablePlants.Count >= maxCount)
                    break;
            }

            return likablePlants;
        }



        public async Task SeedPlantAsync(SeedPlantRequest request)
        {
            //map to plant entity
            var plant = new Plant
            {
                UserId = request.UserId,
                SpeciesName = request.SpeciesName,
                Description = request.Description,
                PlantStage = request.PlantStage,
                WateringNeed = request.WateringNeed,
                LightRequirement = request.LightRequirement,
                Size = request.Size,
                IndoorOutdoor = request.IndoorOutdoor,
                PropagationEase = request.PropagationEase,
                PetFriendly = request.PetFriendly,
                Extras = request.Extras,
                ImageUrl = request.ImageUrl,
                IsTraded = false

            };
            await _plantRepository.AddPlantAsync(plant);
            return;
        }

        public async Task<List<PlantResponse>> GetTradablePlantsLikedByUserFromMeAsync(int userAId, int currentUserId)
        {
            try
            {

                var userA = await _userRepository.GetUserByIdAsync(userAId);
                if (userA == null)
                {
                    throw new NotFoundException($"User with ID {userAId} not found.");
                }
                var userB = await _userRepository.GetUserByIdAsync(currentUserId);
                if (userB == null)
                {
                    throw new NotFoundException($"User with ID {currentUserId} not found.");
                }
                var likedPlants = await _swipeRepository.GetTradableLikedPlantsBySwiperAsync(userAId, currentUserId);
                return BusinessToContractMapper.MapToPlantResponse(likedPlants).ToList();
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plants liked by user with ID {userAId}.");
                throw new BusinessException("Error retrieving plants.", ex);
            }
        }

        public async Task<List<PlantResponse>> GetTradablePlantsLikedByMeFromUserAsync(int userAId, int currentUserId)
        {
            try
            {
                var userA = await _userRepository.GetUserByIdAsync(userAId);
                if (userA == null)
                {
                    throw new NotFoundException($"User with ID {userAId} not found.");
                }
                var userB = await _userRepository.GetUserByIdAsync(currentUserId);
                if (userB == null)
                {
                    throw new NotFoundException($"User with ID {currentUserId} not found.");
                }
                var likedPlants = await _swipeRepository.GetTradableLikedPlantsBySwiperAsync(currentUserId, userAId);
                return BusinessToContractMapper.MapToPlantResponse(likedPlants).ToList();

            } catch (NotFoundException ex)
            {
                throw;
            } catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plants liked by user with ID {userAId}.");
                throw new BusinessException("Error retrieving plants.", ex);
            }
        }

        public async Task MarkPlantAsTradedAsync(int plantId, int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }
                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }

                plant.IsTraded = true;
                await _plantRepository.UpdatePlantAsync(plant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking plant with ID {plantId} as traded.");
                throw new BusinessException("Error marking plant as traded.", ex);
            }
        }
    }
}
