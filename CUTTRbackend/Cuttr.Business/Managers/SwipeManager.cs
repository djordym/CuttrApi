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
        private readonly IMatchRepository _matchRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SwipeManager> _logger;

        public SwipeManager(
            ISwipeRepository swipeRepository,
            IMatchRepository matchRepository,
            IPlantRepository plantRepository,
            IUserRepository userRepository,
            ILogger<SwipeManager> logger)
        {
            _swipeRepository = swipeRepository;
            _matchRepository = matchRepository;
            _plantRepository = plantRepository;
            _userRepository = userRepository;
            _logger = logger;
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
                        // Create a new match
                        var match = new Match
                        {
                            PlantId1 = Math.Min(request.SwiperPlantId, request.SwipedPlantId),
                            PlantId2 = Math.Max(request.SwiperPlantId, request.SwipedPlantId),
                            UserId1 = swiperPlant.UserId,
                            UserId2 = swipedPlant.UserId
                        };

                        var createdMatch = await _matchRepository.AddMatchAsync(match);

                        // Map to MatchResponse
                        response.Match = BusinessToContractMapper.MapToMatchResponse(createdMatch);
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
                // Retrieve user's plants
                var userPlants = await _plantRepository.GetPlantsByUserIdAsync(userId);
                if (userPlants == null || !userPlants.Any())
                    throw new BusinessException("User has no plants in the gallery.");

                // Retrieve all plants except user's own
                var allPlants = await _plantRepository.GetAllPlantsAsync();
                var otherPlants = allPlants.Where(p => p.UserId != userId).ToList();

                var likablePlants = new List<PlantResponse>();

                foreach (var plant in otherPlants)
                {
                    // Check if user has at least one plant that hasn't interacted with this plant
                    bool hasUninteractedPlant = await Task.WhenAll(userPlants.Select(async up =>
                    !await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId)
                    )).Any(result => result);

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

        // Existing methods...
    }
}
