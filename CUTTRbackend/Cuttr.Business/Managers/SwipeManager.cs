using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
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
    public class SwipeManager : ISwipeManager
    {
        private readonly ISwipeRepository _swipeRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly ILogger<SwipeManager> _logger;
        private readonly IUserRepository _userRepository;

        public SwipeManager(
            ISwipeRepository swipeRepository,
            IPlantRepository plantRepository,
            IUserRepository userRepository,
            ILogger<SwipeManager> logger)
        {
            _swipeRepository = swipeRepository;
            _plantRepository = plantRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests, int userId)
        {
            var responses = new List<SwipeResponse>();

            foreach (var request in requests)
            {
                try
                {
                    // Validate that both plants exist
                    var swiperPlant = await _plantRepository.GetPlantByIdAsync(request.SwiperPlantId);
                    if (swiperPlant == null)
                        throw new NotFoundException($"Swiper plant with ID {request.SwiperPlantId} not found.");

                    var swipedPlant = await _plantRepository.GetPlantByIdAsync(request.SwipedPlantId);
                    if (swipedPlant == null)
                        throw new NotFoundException($"Swiped plant with ID {request.SwipedPlantId} not found.");

                    // Validate that the user exists
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    // Validate that the swiper plant belongs to the user
                    if (swiperPlant.UserId != userId)
                        throw new Exceptions.UnauthorizedAccessException("Swiper plant does not belong to the user.");

                    // Map request to Swipe entity
                    var swipe = ContractToBusinessMapper.MapToSwipe(request);

                    await _swipeRepository.AddSwipeAsync(swipe);

                    // Check for a mutual like (match)
                    Swipe oppositeSwipe = null;

                    if (request.IsLike)
                    {
                        oppositeSwipe = await _swipeRepository.GetSwipeAsync(
                            request.SwipedPlantId,
                            request.SwiperPlantId,
                            true);
                    }

                    var response = new SwipeResponse { IsMatch = oppositeSwipe != null };

                    if (response.IsMatch)
                    {
                        // Order plants based on UserId to ensure Plant1 belongs to User1
                        bool isSwiperUserFirst = swiperPlant.UserId < swipedPlant.UserId;

                        var match = new Match
                        {
                            PlantId1 = isSwiperUserFirst ? swiperPlant.PlantId : swipedPlant.PlantId,
                            PlantId2 = isSwiperUserFirst ? swipedPlant.PlantId : swiperPlant.PlantId,
                            UserId1 = isSwiperUserFirst ? swiperPlant.UserId : swipedPlant.UserId,
                            UserId2 = isSwiperUserFirst ? swipedPlant.UserId : swiperPlant.UserId,
                            CreatedAt = DateTime.UtcNow
                        };
                    }

                    responses.Add(response);
                }
                catch (NotFoundException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error recording swipe.");
                    throw new BusinessException("Error recording swipe.", ex);
                }
            }

            return responses;
        }

        public async Task<List<PlantResponse>> GetLikablePlantsAsync(int userId)
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
                    bool hasUninteractedPlant = (await Task.WhenAll(
                        userPlants.Select(async up =>
                            !await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId)
                        ))
                    ).Any(result => result);

                    if (hasUninteractedPlant)
                    {
                        likablePlants.Add(BusinessToContractMapper.MapToPlantResponse(plant));
                    }
                }

                return likablePlants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving likable plants.");
                throw new BusinessException("Error retrieving likable plants.", ex);
            }
        }


    }
}
