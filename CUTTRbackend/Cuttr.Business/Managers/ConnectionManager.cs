﻿using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities; // Connection, ConnectionMessage, TradeProposal (domain models)
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Mappers; // Example for mapping domain <-> DTO
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnauthorizedAccessException = Cuttr.Business.Exceptions.UnauthorizedAccessException;

namespace Cuttr.Business.Managers
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly IConnectionRepository _connectionRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ITradeProposalRepository _tradeProposalRepository;
        private readonly IPlantRepository _plantRepository; // optional if you need to load plants
        private readonly ILogger<ConnectionManager> _logger;

        public ConnectionManager(
            IConnectionRepository connectionRepository,
            IMessageRepository messageRepository,
            ITradeProposalRepository tradeProposalRepository,
            IPlantRepository plantRepository,
            ILogger<ConnectionManager> logger)
        {
            _connectionRepository = connectionRepository;
            _messageRepository = messageRepository;
            _tradeProposalRepository = tradeProposalRepository;
            _plantRepository = plantRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ConnectionResponse>> GetConnectionsForUserAsync(int userId)
        {
            _logger.LogInformation("Fetching connections for user with ID {UserId}.", userId);
            try
            {
                var connections = await _connectionRepository.GetConnectionsByUserIdAsync(userId);
                if (connections == null || !connections.Any())
                {
                    _logger.LogInformation("No connections found for user with ID {UserId}.", userId);
                    return Enumerable.Empty<ConnectionResponse>();
                }

                // Materialize the list to avoid deferred execution
                var connResponses = connections
                    .Select(conn => BusinessToContractMapper.MapToConnectionResponse(conn))
                    .ToList();

                // Sequentially set NumberOfMatches
                foreach (var conn in connResponses)
                {
                    conn.NumberOfMatches = await _connectionRepository.GetNumberOfMatchesAsync(conn.ConnectionId);
                }

                _logger.LogInformation("Successfully fetched connections for user with ID {UserId}.", userId);
                return connResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching connections for user with ID {UserId}.", userId);
                throw new BusinessException("An error occurred while fetching connections.", ex);
            }
        }


        public async Task<ConnectionResponse> GetConnectionByIdAsync(int connectionId, int userId)
        {
            _logger.LogInformation("Fetching connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
            try
            {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, userId);

                var response = BusinessToContractMapper.MapToConnectionResponse(connection);
                _logger.LogInformation("Successfully fetched connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt by user ID {UserId} to connection ID {ConnectionId}.", userId, connectionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
                throw new BusinessException("An error occurred while fetching the connection.", ex);
            }
        }

        // Uncomment and refactor these methods similarly if needed
        /*
        public async Task<IEnumerable<ConnectionMessageResponse>> GetMessagesAsync(int connectionId, int userId)
        {
            _logger.LogInformation("Fetching messages for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
            try
            {
                var connection = await _connectionRepository.GetByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, userId);

                var messages = await _messageRepository.GetMessagesByConnectionIdAsync(connectionId);
                var response = messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => BusinessToContractMapper.MapConnectionMessageToResponse(m));

                _logger.LogInformation("Successfully fetched messages for connection ID {ConnectionId}.", connectionId);
                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt by user ID {UserId} to messages in connection ID {ConnectionId}.", userId, connectionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
                throw new BusinessException("An error occurred while fetching messages.", ex);
            }
        }

        public async Task<MessageResponse> SendMessageAsync(int connectionId, int senderUserId, MessageRequest request)
        {
            _logger.LogInformation("Sending message in connection ID {ConnectionId} by user ID {SenderUserId}.", connectionId, senderUserId);
            try
            {
                var connection = await _connectionRepository.GetByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, senderUserId);

                var newMessage = new ConnectionMessage
                {
                    ConnectionId = connectionId,
                    SenderUserId = senderUserId,
                    MessageText = request.MessageText,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _messageRepository.CreateAsync(newMessage);
                var response = BusinessToContractMapper.MapConnectionMessageToResponse(newMessage);

                _logger.LogInformation("Successfully sent message in connection ID {ConnectionId} by user ID {SenderUserId}.", connectionId, senderUserId);
                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt by user ID {SenderUserId} to send message in connection ID {ConnectionId}.", senderUserId, connectionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message in connection ID {ConnectionId} by user ID {SenderUserId}.", connectionId, senderUserId);
                throw new BusinessException("An error occurred while sending the message.", ex);
            }
        }
        */

        public async Task<IEnumerable<TradeProposalResponse>> GetTradeProposalsAsync(int connectionId, int userId)
        {
            _logger.LogInformation("Fetching trade proposals for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
            try
            {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, userId);

                var proposals = await _tradeProposalRepository.GetByConnectionIdAsync(connectionId);
                var response = proposals
                    .OrderByDescending(tp => tp.CreatedAt)
                    .Select(tp => BusinessToContractMapper.MapToTradeProposalResponse(tp));

                _logger.LogInformation("Successfully fetched trade proposals for connection ID {ConnectionId}.", connectionId);
                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt by user ID {UserId} to trade proposals in connection ID {ConnectionId}.", userId, connectionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trade proposals for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
                throw new BusinessException("An error occurred while fetching trade proposals.", ex);
            }
        }

        public async Task<TradeProposalResponse> CreateTradeProposalAsync(int connectionId, int userId, TradeProposalRequest request)
        {
            _logger.LogInformation("Creating trade proposal in connection ID {ConnectionId} by user ID {UserId}.", connectionId, userId);
            try
            {
                // 1) Ensure connection exists & user is participant
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, userId);

                // 2) Validate or parse the request.
                // Example validation can be added here as needed

                // 3) Create domain model
                var newProposal = new TradeProposal
                {
                    ConnectionId = connectionId,
                    TradeProposalStatus = Enums.TradeProposalStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    PlantIdsProposedByUser1 = request.UserPlantIds,
                    PlantIdsProposedByUser2 = request.OtherPlantIds
                };

                await _tradeProposalRepository.CreateAsync(newProposal);
                _logger.LogInformation("Trade proposal created with ID {ProposalId} in connection ID {ConnectionId}.", newProposal.TradeProposalId, connectionId);

                // 4) Return response
                var response = BusinessToContractMapper.MapToTradeProposalResponse(newProposal);
                _logger.LogInformation("Successfully created trade proposal with ID {ProposalId}.", newProposal.TradeProposalId);
                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trade proposal in connection ID {ConnectionId} by user ID {UserId}.", connectionId, userId);
                throw new BusinessException("An error occurred while creating the trade proposal.", ex);
            }
        }

        public async Task UpdateTradeProposalStatusAsync(int connectionId, int proposalId, int userId, UpdateTradeProposalStatusRequest request)
        {
            _logger.LogInformation("Updating trade proposal ID {ProposalId} status to {NewStatus} in connection ID {ConnectionId} by user ID {UserId}.", proposalId, request.NewStatus, connectionId, userId);
            try
            {
                // 1) Ensure connection is valid & user is participant
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
                    throw new NotFoundException($"Connection with ID {connectionId} not found.");
                }

                EnsureUserIsParticipantOfConnection(connection, userId);

                // 2) Load the proposal
                var proposal = await _tradeProposalRepository.GetByIdAsync(proposalId);
                if (proposal == null || proposal.ConnectionId != connectionId)
                {
                    _logger.LogWarning("Trade proposal with ID {ProposalId} not found in connection ID {ConnectionId}.", proposalId, connectionId);
                    throw new NotFoundException($"Trade proposal with ID {proposalId} not found in connection {connectionId}.");
                }

                // 3) Update status according to request.NewStatus
                switch (request.NewStatus)
                {
                    case Enums.TradeProposalStatus.Accepted:
                        proposal.TradeProposalStatus = Enums.TradeProposalStatus.Accepted;
                        proposal.AcceptedAt = DateTime.UtcNow;
                        proposal.DeclinedAt = null;
                        proposal.CompletedAt = null;
                        break;

                    case Enums.TradeProposalStatus.Rejected:
                        proposal.TradeProposalStatus = Enums.TradeProposalStatus.Rejected;
                        proposal.DeclinedAt = DateTime.UtcNow;
                        proposal.AcceptedAt = null;
                        proposal.CompletedAt = null;
                        break;

                    case Enums.TradeProposalStatus.Completed:
                        if (proposal.TradeProposalStatus != Enums.TradeProposalStatus.Accepted)
                        {
                            _logger.LogWarning("Cannot mark proposal ID {ProposalId} as 'Completed' because it is not 'Accepted'.", proposalId);
                            throw new BusinessException("Cannot mark proposal as 'Completed' if it has not been 'Accepted'.");
                        }
                        proposal.TradeProposalStatus = Enums.TradeProposalStatus.Completed;
                        proposal.CompletedAt = DateTime.UtcNow;
                        // Optionally mark plants as traded
                        await MarkPlantsAsTraded(proposal);
                        break;

                    default:
                        _logger.LogWarning("Invalid trade proposal status '{NewStatus}' provided for proposal ID {ProposalId}.", request.NewStatus, proposalId);
                        throw new BusinessException($"Unknown status '{request.NewStatus}'. Allowed: Accepted, Rejected, Completed.");
                }

                await _tradeProposalRepository.UpdateAsync(proposal);
                _logger.LogInformation("Successfully updated trade proposal ID {ProposalId} to status {NewStatus}.", proposalId, request.NewStatus);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt by user ID {UserId} to update trade proposal ID {ProposalId} in connection ID {ConnectionId}.", userId, proposalId, connectionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trade proposal ID {ProposalId} in connection ID {ConnectionId} by user ID {UserId}.", proposalId, connectionId, userId);
                throw new BusinessException("An error occurred while updating the trade proposal.", ex);
            }
        }

        private void EnsureUserIsParticipantOfConnection(Connection connection, int userId)
        {
            if (connection.UserId1 != userId && connection.UserId2 != userId)
            {
                _logger.LogWarning("User ID {UserId} is not a participant of connection ID {ConnectionId}.", userId, connection.ConnectionId);
                throw new UnauthorizedAccessException(
                    $"User {userId} is not a participant of connection {connection.ConnectionId}."
                );
            }
        }

        private async Task MarkPlantsAsTraded(TradeProposal proposal)
        {
            //_logger.LogInformation("Marking plants as traded for trade proposal ID {ProposalId}.", proposal.TradeProposalId);
            //try
            //{
            //    var plantIdsUser1 = proposal.PlantIdsProposedByUser1?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            //    var plantIdsUser2 = proposal.PlantIdsProposedByUser2?.Split(',').Select(int.Parse).ToList() ?? new List<int>();

            //    var allPlantIds = plantIdsUser1.Concat(plantIdsUser2).Distinct();

            //    var plants = await _plantRepository.GetPlantsByIdsAsync(allPlantIds);
            //    foreach (var plant in plants)
            //    {
            //        plant.IsClosed = true; // Assuming there's an IsClosed property
            //        // Additional logic can be added here as needed
            //    }

            //    await _plantRepository.UpdatePlantsAsync(plants);
            //    _logger.LogInformation("Successfully marked plants as traded for trade proposal ID {ProposalId}.", proposal.TradeProposalId);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error marking plants as traded for trade proposal ID {ProposalId}.", proposal.TradeProposalId);
            //    throw new BusinessException("An error occurred while marking plants as traded.", ex);
            //}
        }
    }
}
