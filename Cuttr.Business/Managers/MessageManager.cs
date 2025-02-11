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
    public class MessageManager : IMessageManager
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConnectionRepository _connectionRepository;
        private readonly ILogger<MessageManager> _logger;

        public MessageManager(
            IMessageRepository messageRepository,
            IConnectionRepository matchRepository,
            ILogger<MessageManager> logger)
        {
            _messageRepository = messageRepository;
            _connectionRepository = matchRepository;
            _logger = logger;
        }

        public async Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId, int connectionId)
        {
            try
            {
                // Validate that the match exists
                var match = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (match == null)
                    throw new NotFoundException($"Match with ID {connectionId} not found.");

                // Validate that the sender user belongs to the match
                if (match.UserId1 != senderUserId && match.UserId2 != senderUserId)
                    throw new BusinessException("Sender user is not part of the match.");

                // Create Message entity
                var message = ContractToBusinessMapper.MapToMessage(request, senderUserId, connectionId);

                var createdMessage = await _messageRepository.AddMessageAsync(message);

                // Map to MessageResponse
                return BusinessToContractMapper.MapToMessageResponse(createdMessage);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message.");
                throw new BusinessException("Error sending message.", ex);
            }
        }

        public async Task<IEnumerable<MessageResponse>> GetMessagesByConnectionIdAsync(int connectionId, int userId)
        {
            try
            {
                // Validate that the match exists
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    throw new NotFoundException($"Match with ID {connectionId} not found.");

                // Validate that the user is part of the match
                if (connection.UserId1 != userId && connection.UserId2 != userId)
                    throw new Exceptions.UnauthorizedAccessException("User is not part of the match.");

                var messages = await _messageRepository.GetMessagesByConnectionIdAsync(connectionId);

                return BusinessToContractMapper.MapToMessageResponse(messages);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exceptions.UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for match with ID {connectionId}.");
                throw new BusinessException("Error retrieving messages.", ex);
            }
        }
    }
}
