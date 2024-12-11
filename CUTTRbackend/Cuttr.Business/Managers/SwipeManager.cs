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

        public async Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests)
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
                // Retrieve the user to get their location
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new BusinessException("User not found.");

                // Ensure user location is set, if not, fallback or throw
                if (user.Preferences == null)
                    throw new BusinessException("User preferences not found.");
                int radius;
                if (user.Preferences.SearchRadius == null)
                {
                    radius = 9999;
                } else
                {
                    radius = user.Preferences.SearchRadius;
                }

                // Default location if not set, or throw if you require a set location
                if (user.LocationLatitude == null || user.LocationLongitude == null)
                    throw new BusinessException("User location not set.");

                double userLat = user.LocationLatitude.Value;
                double userLon = user.LocationLongitude.Value;

                // Get only plants within the user's search radius
                var candidatePlants = await _plantRepository.GetPlantsWithinRadiusAsync(userLat, userLon, radius);

                // Exclude user’s own plants
                candidatePlants = candidatePlants.Where(p => p.UserId != userId);

                // Retrieve user's own plants
                var userPlants = await _plantRepository.GetPlantsByUserIdAsync(userId);

                var likablePlants = new List<PlantResponse>();

                foreach (var plant in candidatePlants)
                {
                    bool hasUninteractedPlant = (await Task.WhenAll(userPlants.Select(async up =>
                        !await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId)
                    ))).Any(result => result);

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
