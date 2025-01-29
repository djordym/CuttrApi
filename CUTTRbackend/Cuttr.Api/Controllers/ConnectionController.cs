using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Cuttr.Api.Common; // For User.GetUserId()
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Contracts.Inputs;    // For your request models
using Cuttr.Business.Contracts.Outputs;   // For your response models
using Cuttr.Business.Exceptions;
using UnauthorizedAccessException = Cuttr.Business.Exceptions.UnauthorizedAccessException;
using Cuttr.Business.Managers;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/connections")]
    [Authorize]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger<ConnectionController> _logger;
        private readonly IMessageManager _messageManager;

        public ConnectionController(
            IConnectionManager connectionManager,
            ILogger<ConnectionController> logger,
            IMessageManager messageManager
        )
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _messageManager = messageManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyConnections()
        {
            try
            {
                int userId = User.GetUserId();
                var connections = await _connectionManager.GetConnectionsForUserAsync(userId);
                return Ok(connections); // e.g. List<ConnectionResponse>
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving connections.");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving connections.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{connectionId}")]
        public async Task<IActionResult> GetConnection(int connectionId)
        {
            try
            {
                int userId = User.GetUserId();
                var connection = await _connectionManager.GetConnectionByIdAsync(connectionId, userId);
                return Ok(connection); // e.g. ConnectionResponse
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to connection.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving connection with ID {connectionId}.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving the connection.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{connectionId}/proposals")]
        public async Task<IActionResult> GetTradeProposals(int connectionId)
        {
            try
            {
                int userId = User.GetUserId();
                var proposals = await _connectionManager.GetTradeProposalsAsync(connectionId, userId);
                return Ok(proposals); // e.g. List<TradeProposalResponse>
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to trade proposals.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving trade proposals for connection {connectionId}.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving trade proposals.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost("{connectionId}/proposals")]
        public async Task<IActionResult> CreateTradeProposal(int connectionId, [FromBody] TradeProposalRequest request)
        {
            try
            {
                int userId = User.GetUserId();
                var createdProposal = await _connectionManager.CreateTradeProposalAsync(connectionId, userId, request);
                return Ok(createdProposal); // e.g. TradeProposalResponse
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to create a trade proposal.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error creating trade proposal.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating the trade proposal.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{connectionId}/proposals/{proposalId}/status")]
        public async Task<IActionResult> UpdateTradeProposalStatus(
            int connectionId,
            int proposalId,
            [FromBody] UpdateTradeProposalStatusRequest request
        )
        {
            try
            {
                int userId = User.GetUserId();
                await _connectionManager.UpdateTradeProposalStatusAsync(
                    connectionId,
                    proposalId,
                    userId,
                    request
                );
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Proposal with ID {proposalId} or Connection with ID {connectionId} not found.");
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to update trade proposal status.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error updating trade proposal status.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the trade proposal status.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        

        // GET: api/matches/{matchId}/messages
        [HttpGet("{connectionId}/messages")]
        public async Task<IActionResult> GetMessages(int connectionId)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();

                var messages = await _messageManager.GetMessagesByConnectionIdAsync(connectionId, userId);
                return Ok(messages);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Match with ID {connectionId} not found.");
                return NotFound(ex.Message);
            }
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to messages.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for match with ID {connectionId}.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while accessing messages.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
        // POST: api/messages/me
        [HttpPost("{connectionId}/messages/user/me")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request, int connectionId)
        {
            int senderUserId = 0;
            try
            {
                senderUserId = User.GetUserId();

                MessageResponse messageResponse = await _messageManager.SendMessageAsync(request, senderUserId, connectionId);
                return Ok(messageResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Match not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error sending message.");
                return BadRequest(ex.Message);
            }
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while sending message.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
