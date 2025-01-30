using Azure;
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
        private readonly IConnectionRepository _connectionRepository;

        public SwipeManager(
            ISwipeRepository swipeRepository,
            IPlantRepository plantRepository,
            IUserRepository userRepository,
            ILogger<SwipeManager> logger,
        IMatchRepository matchRepository,
        IConnectionRepository connectionRepository)
        {
            _swipeRepository = swipeRepository;
            _plantRepository = plantRepository;
            _logger = logger;
            _userRepository = userRepository;
            _matchRepository = matchRepository;
            _connectionRepository = connectionRepository;
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

                    SwipeResponse swipeResponse = new SwipeResponse { IsMatch = oppositeSwipe != null };

                    // 5. If there's a match, create it, also create a connection if there is not yet one
                    // 5. If there's a match, create it, also create a connection if there is not yet one
                    if (swipeResponse.IsMatch)
                    {
                        // CONNECTION
                        // Check if a connection already exists for these two users
                        var swiperUserId = swiperPlant.UserId;
                        var swipedUserId = swipedPlant.UserId;

                        var existingConnection = await _connectionRepository.GetConnectionByUsersAsync(swiperUserId, swipedUserId);

                        Connection currentConnection;
                        if (existingConnection == null)
                        {
                            // No existing connection -> create a new one
                            var newConnection = new Connection
                            {
                                UserId1 = swiperUserId,
                                UserId2 = swipedUserId,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };

                            currentConnection = await _connectionRepository.CreateConnectionAsync(newConnection);

                            // Map domain object to a response for the client
                            swipeResponse.Connection = BusinessToContractMapper.MapToConnectionResponse(currentConnection);
                        }
                        else
                        {
                            // A connection already exists
                            currentConnection = existingConnection;
                            swipeResponse.Connection = BusinessToContractMapper.MapToConnectionResponse(existingConnection);
                        }

                        // Ensure the connection is valid
                        if (currentConnection == null || currentConnection.ConnectionId == 0)
                            throw new BusinessException("Connection not properly established.");

                        // MATCH
                        // Assign PlantId1 and PlantId2 based on the ordering in the connection
                        int plantId1, plantId2;

                        if (currentConnection.UserId1 == swiperUserId)
                        {
                            plantId1 = swiperPlant.PlantId;
                            plantId2 = swipedPlant.PlantId;
                        }
                        else
                        {
                            plantId1 = swipedPlant.PlantId;
                            plantId2 = swiperPlant.PlantId;
                        }

                        var match = new Match
                        {
                            PlantId1 = plantId1,
                            PlantId2 = plantId2,
                            ConnectionId = currentConnection.ConnectionId,
                            CreatedAt = DateTime.UtcNow
                        };

                        var addedMatch = await _matchRepository.AddMatchAsync(match);
                        if (addedMatch != null)
                        {
                            swipeResponse.Match = BusinessToContractMapper.MapToMatchResponse(addedMatch);
                        }
                    }

                    responses.Add(swipeResponse);
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

    }
}
