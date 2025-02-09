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

                var plants = await _plantRepository.GetPlantsByUserIdAsync(userId);

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
            try
            {
                // 1. Retrieve the user to get location & preferences
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new BusinessException("User not found.");

                if (user.Preferences == null)
                    throw new BusinessException("User preferences not found.");

                if (user.LocationLatitude == null || user.LocationLongitude == null)
                    throw new BusinessException("User location not set.");

                // 2. Determine radius or fallback
                int radius = user.Preferences.SearchRadius > 0
                             ? user.Preferences.SearchRadius
                             : 10000; // or 9999, etc.

                // 3. Retrieve the candidate plants in radius
                var candidatePlants = await _plantRepository.GetPlantsWithinRadiusAsync(
                                          user.LocationLatitude.Value,
                                          user.LocationLongitude.Value,
                                          radius);

                // 4. Exclude user’s own plants
                candidatePlants = candidatePlants.Where(p => p.UserId != userId);

                // 5. Apply preference filters if lists are non-empty
                if (user.Preferences.PreferedPlantStage != null && user.Preferences.PreferedPlantStage.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => user.Preferences.PreferedPlantStage.Contains(p.PlantStage));
                }

                if (user.Preferences.PreferedPlantCategory != null && user.Preferences.PreferedPlantCategory.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.PlantCategory.HasValue
                                    && user.Preferences.PreferedPlantCategory.Contains(p.PlantCategory.Value));
                }

                if (user.Preferences.PreferedWateringNeed != null && user.Preferences.PreferedWateringNeed.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.WateringNeed.HasValue
                                    && user.Preferences.PreferedWateringNeed.Contains(p.WateringNeed.Value));
                }

                if (user.Preferences.PreferedLightRequirement != null && user.Preferences.PreferedLightRequirement.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.LightRequirement.HasValue
                                    && user.Preferences.PreferedLightRequirement.Contains(p.LightRequirement.Value));
                }

                if (user.Preferences.PreferedSize != null && user.Preferences.PreferedSize.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.Size.HasValue
                                    && user.Preferences.PreferedSize.Contains(p.Size.Value));
                }

                if (user.Preferences.PreferedIndoorOutdoor != null && user.Preferences.PreferedIndoorOutdoor.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.IndoorOutdoor.HasValue
                                    && user.Preferences.PreferedIndoorOutdoor.Contains(p.IndoorOutdoor.Value));
                }

                if (user.Preferences.PreferedPropagationEase != null && user.Preferences.PreferedPropagationEase.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.PropagationEase.HasValue
                                    && user.Preferences.PreferedPropagationEase.Contains(p.PropagationEase.Value));
                }

                if (user.Preferences.PreferedPetFriendly != null && user.Preferences.PreferedPetFriendly.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.PetFriendly.HasValue
                                    && user.Preferences.PreferedPetFriendly.Contains(p.PetFriendly.Value));
                }


                if (user.Preferences.PreferedExtras != null && user.Preferences.PreferedExtras.Any())
                {
                    candidatePlants = candidatePlants
                        .Where(p => p.Extras != null && p.Extras.Any(e => user.Preferences.PreferedExtras.Contains(e)));
                }

                // 6. Retrieve user's own plants (to check if already swiped)
                var userPlants = await _plantRepository.GetPlantsByUserIdAsync(userId);

                var likablePlants = new List<PlantResponse>();

                // 7. Exclude plants that have already been swiped
                foreach (var plant in candidatePlants)
                {
                    bool hasUninteractedPlant = false;
                    foreach (var up in userPlants)
                    {
                        // Check if this user plant has NOT swiped on the candidate plant
                        bool hasSwipe = await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId);
                        if (!hasSwipe)
                        {
                            hasUninteractedPlant = true;
                            break; // Found an uninteracted swipe; no need to check further for this plant
                        }
                    }

                    if (hasUninteractedPlant)
                    {
                        likablePlants.Add(BusinessToContractMapper.MapToPlantResponse(plant));
                    }

                    if (likablePlants.Count >= maxCount)
                        break; // Reached the maximum count

                }
                return likablePlants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving likable plants.");
                throw new BusinessException("Error retrieving likable plants.", ex);
            }
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
                ImageUrl = request.ImageUrl

            };
            await _plantRepository.AddPlantAsync(plant);
            return;
        }

        public async Task<List<PlantResponse>> GetPlantsLikedByUserFromMeAsync(int userAId, int currentUserId)
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
                var likedPlants = await _swipeRepository.GetLikedPlantsBySwiperAsync(userAId, currentUserId);
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

        public async Task<List<PlantResponse>> GetPlantsLikedByMeFromUserAsync(int userAId, int currentUserId)
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
                var likedPlants = await _swipeRepository.GetLikedPlantsBySwiperAsync(currentUserId, userAId);
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
