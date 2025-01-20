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
        private readonly IMatchRepository _matchRepository;

        public SwipeManager(
            ISwipeRepository swipeRepository,
            IPlantRepository plantRepository,
            IUserRepository userRepository,
            ILogger<SwipeManager> logger,
        IMatchRepository matchRepository)
        {
            _swipeRepository = swipeRepository;
            _plantRepository = plantRepository;
            _logger = logger;
            _userRepository = userRepository;
            _matchRepository = matchRepository;
        }

        public async Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests, int userId)
        {
            var responses = new List<SwipeResponse>();

            foreach (var request in requests)
            {
                try
                {
                    // 1. Validate plants and user
                    var swiperPlant = await _plantRepository.GetPlantByIdAsync(request.SwiperPlantId);
                    if (swiperPlant == null)
                        throw new NotFoundException($"Swiper plant with ID {request.SwiperPlantId} not found.");

                    var swipedPlant = await _plantRepository.GetPlantByIdAsync(request.SwipedPlantId);
                    if (swipedPlant == null)
                        throw new NotFoundException($"Swiped plant with ID {request.SwipedPlantId} not found.");

                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (swiperPlant.UserId != userId)
                        throw new Exceptions.UnauthorizedAccessException("Swiper plant does not belong to the user.");

                    // 2. See if a Swipe already exists for this pair.
                    var existingSwipe = await _swipeRepository.GetSwipeForPairAsync(
                        request.SwiperPlantId,
                        request.SwipedPlantId
                    );

                    // 3. Only add or update if needed.
                    if (existingSwipe == null)
                    {
                        // No existing record -> always insert
                        var newSwipe = ContractToBusinessMapper.MapToSwipe(request);
                        await _swipeRepository.AddSwipeAsync(newSwipe);
                    }
                    else
                    {
                        // There's an existing record
                        // The only scenario we want to update is if existing is a "dislike"
                        // and the user is now swiping "like" (i.e. from false -> true).
                        if (!existingSwipe.IsLike && request.IsLike)
                        {
                            existingSwipe.IsLike = true;
                            await _swipeRepository.UpdateSwipeAsync(existingSwipe);
                        }
                        // else -> do nothing in all other scenarios
                    }

                    // 4. Check for mutual like (match) only if the new/updated request is a like
                    //    and only if we just recorded (or updated to) a like.
                    //    So let's confirm the final state is indeed "like".
                    bool finalIsLike = request.IsLike || // if we inserted a brand new "like"
                                       (existingSwipe != null && existingSwipe.IsLike); // or if updated

                    Swipe oppositeSwipe = null;
                    if (finalIsLike)
                    {
                        oppositeSwipe = await _swipeRepository.GetSwipeAsync(
                            request.SwipedPlantId,
                            request.SwiperPlantId,
                            true
                        );
                    }

                    var response = new SwipeResponse { IsMatch = oppositeSwipe != null };

                    // 5. If there's a match, create it
                    if (response.IsMatch)
                    {
                        bool isSwiperUserFirst = swiperPlant.UserId < swipedPlant.UserId;

                        var match = new Match
                        {
                            PlantId1 = isSwiperUserFirst ? swiperPlant.PlantId : swipedPlant.PlantId,
                            PlantId2 = isSwiperUserFirst ? swipedPlant.PlantId : swiperPlant.PlantId,
                            UserId1 = isSwiperUserFirst ? swiperPlant.UserId : swipedPlant.UserId,
                            UserId2 = isSwiperUserFirst ? swipedPlant.UserId : swiperPlant.UserId,
                            CreatedAt = DateTime.UtcNow
                        };

                        var addedMatch = await _matchRepository.AddMatchAsync(match);
                        response.Match = BusinessToContractMapper.MapToMatchResponse(addedMatch);
                    }

                    responses.Add(response);
                }
                catch (NotFoundException)
                {
                    // Rethrow
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


    }
}
