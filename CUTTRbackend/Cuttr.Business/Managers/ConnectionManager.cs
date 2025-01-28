using Cuttr.Business.Contracts.Inputs;
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
            var connections = await _connectionRepository.GetConnectionsByUserIdAsync(userId);
            // Map to response DTO
            var connResponses = connections.Select(conn => BusinessToContractMapper.MapToConnectionResponse(conn));
            //add number of matches to each response
            foreach (var conn in connResponses)
            {
                conn.NumberOfMatches = await _connectionRepository.GetNumberOfMatchesAsync(conn.ConnectionId);
            }
            return connResponses;
        }

        public async Task<ConnectionResponse> GetConnectionByIdAsync(int connectionId, int userId)
        {
            var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
            if (connection == null)
            {
                throw new NotFoundException($"Connection with ID {connectionId} not found.");
            }

            EnsureUserIsParticipantOfConnection(connection, userId);

            return BusinessToContractMapper.MapToConnectionResponse(connection);
        }

        ///// <summary>
        ///// Returns all messages for a given connection, ensuring the user is authorized to view them.
        ///// </summary>
        //public async Task<IEnumerable<ConnectionMessageResponse>> GetMessagesAsync(int connectionId, int userId)
        //{
        //    var connection = await _connectionRepository.GetByIdAsync(connectionId);
        //    if (connection == null)
        //    {
        //        throw new NotFoundException($"Connection with ID {connectionId} not found.");
        //    }

        //    EnsureUserIsParticipantOfConnection(connection, userId);

        //    var messages = await _messageRepository.GetMessagesByConnectionIdAsync(connectionId);
        //    return messages
        //        .OrderBy(m => m.SentAt)
        //        .Select(m => BusinessToContractMapper.MapConnectionMessageToResponse(m));
        //}

        ///// <summary>
        ///// Creates and returns a new message in the specified connection.
        ///// </summary>
        //public async Task<MessageResponse> SendMessageAsync(
        //    int connectionId,
        //    int senderUserId,
        //    MessageRequest request)
        //{
        //    var connection = await _connectionRepository.GetByIdAsync(connectionId);
        //    if (connection == null)
        //    {
        //        throw new NotFoundException($"Connection with ID {connectionId} not found.");
        //    }

        //    EnsureUserIsParticipantOfConnection(connection, senderUserId);

        //    // Create domain entity
        //    var newMessage = new ConnectionMessage
        //    {
        //        ConnectionId = connectionId,
        //        SenderUserId = senderUserId,
        //        MessageText = request.MessageText,
        //        SentAt = DateTime.UtcNow,
        //        IsRead = false
        //    };

        //    await _messageRepository.CreateAsync(newMessage);

        //    // Map back to response
        //    return BusinessToContractMapper.MapConnectionMessageToResponse(newMessage);
        //}

        public async Task<IEnumerable<TradeProposalResponse>> GetTradeProposalsAsync(int connectionId, int userId)
        {
            var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
            if (connection == null)
            {
                throw new NotFoundException($"Connection with ID {connectionId} not found.");
            }

            EnsureUserIsParticipantOfConnection(connection, userId);

            IEnumerable<TradeProposal> proposals = await _tradeProposalRepository.GetByConnectionIdAsync(connectionId);
            return proposals
                .OrderByDescending(tp => tp.CreatedAt)
                .Select(tp => BusinessToContractMapper.MapToTradeProposalResponse(tp));
        }

        public async Task<TradeProposalResponse> CreateTradeProposalAsync(
            int connectionId,
            int userId,
            TradeProposalRequest request)
        {
            // 1) Ensure connection exists & user is participant
            var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
            if (connection == null)
            {
                throw new NotFoundException($"Connection with ID {connectionId} not found.");
            }

            EnsureUserIsParticipantOfConnection(connection, userId);

            // 2) Validate or parse the request. 
            // For example, you might parse the user’s plant IDs, or ensure the other user’s plants are correct, etc.
            // This snippet is simplified.

            // 3) Create domain model
            var newProposal = new TradeProposal
            {
                ConnectionId = connectionId,
                TradeProposalStatus = Enums.TradeProposalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                // Convert request plant ID lists to strings or JSON
                PlantIdsProposedByUser1 = (userId == connection.UserId1) ? request.UserPlantIds : request.OtherPlantIds,
                PlantIdsProposedByUser2 = (userId == connection.UserId2) ? request.UserPlantIds : request.OtherPlantIds,

            };

            await _tradeProposalRepository.CreateAsync(newProposal);

            // 4) Return response
            return BusinessToContractMapper.MapToTradeProposalResponse(newProposal);
        }

        public async Task UpdateTradeProposalStatusAsync(
            int connectionId,
            int proposalId,
            int userId,
            UpdateTradeProposalStatusRequest request)
        {
            // 1) Ensure connection is valid & user is participant
            var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
            if (connection == null)
            {
                throw new NotFoundException($"Connection with ID {connectionId} not found.");
            }
            EnsureUserIsParticipantOfConnection(connection, userId);

            // 2) Load the proposal
            var proposal = await _tradeProposalRepository.GetByIdAsync(proposalId);
            if (proposal == null || proposal.ConnectionId != connectionId)
            {
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
                    // Typically you might clear accepted/completed 
                    // because once it's declined, the user can't keep it "accepted"
                    proposal.AcceptedAt = null;
                    proposal.CompletedAt = null;
                    break;

                case Enums.TradeProposalStatus.Completed:
                    if (proposal.TradeProposalStatus != Enums.TradeProposalStatus.Accepted)
                    {
                        throw new BusinessException("Cannot mark proposal as 'Completed' if it has not been 'Accepted'.");
                    }
                    proposal.TradeProposalStatus = Enums.TradeProposalStatus.Completed;
                    proposal.CompletedAt = DateTime.UtcNow;
                    // Here you might also mark the relevant plants as "IsClosed = true" or remove them from the user’s inventory.
                    // e.g. MarkPlantsAsTraded(proposal)
                    break;

                default:
                    throw new BusinessException($"Unknown status '{request.NewStatus}'. Allowed: Accepted, Declined, Completed.");
            }

            await _tradeProposalRepository.UpdateAsync(proposal);
        }

        private void EnsureUserIsParticipantOfConnection(Connection connection, int userId)
        {
            if (connection.UserId1 != userId && connection.UserId2 != userId)
            {
                throw new UnauthorizedAccessException(
                    $"User {userId} is not a participant of connection {connection.ConnectionId}."
                );
            }
        }

        private async Task MarkPlantsAsTraded(TradeProposal proposal)
        {
            // parse proposal.ItemsIdsProposedByUser1 / ItemsIdsProposedByUser2
            // load each plant from _plantRepository
            // plant.IsClosed = true;  // or whatever you do
            // update plants
        }
    }
}
