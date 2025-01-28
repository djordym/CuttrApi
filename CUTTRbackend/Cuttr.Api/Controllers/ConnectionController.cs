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

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/connections")]
    [Authorize]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger<ConnectionController> _logger;

        public ConnectionController(
            IConnectionManager connectionManager,
            ILogger<ConnectionController> logger
        )
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        /// <summary>
        /// Get all connections for the logged-in user.
        /// </summary>
        /// <returns>List of connections (user-to-user) that belong to the current user.</returns>
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

        /// <summary>
        /// Get a specific connection by ID (ensures the logged-in user is part of that connection).
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>A single connection with details, or 404 if not found/authorized.</returns>
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

        /// <summary>
        /// Get all messages for a given connection (make sure the user is part of the connection).
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>A list of messages for this connection.</returns>
        //[HttpGet("{connectionId}/messages")]
        //public async Task<IActionResult> GetMessages(int connectionId)
        //{
        //    try
        //    {
        //        int userId = User.GetUserId();
        //        var messages = await _connectionManager.GetMessagesAsync(connectionId, userId);
        //        return Ok(messages); // e.g. List<ConnectionMessageResponse>
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        _logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
        //        return NotFound(ex.Message);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "Unauthorized access to messages.");
        //        return Forbid(ex.Message);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        _logger.LogError(ex, $"Error retrieving messages for connection ID {connectionId}.");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred while accessing messages.");
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}

        ///// <summary>
        ///// Send a message in the given connection (the sender is the current user).
        ///// </summary>
        ///// <param name="connectionId"></param>
        ///// <param name="request">Contains the message text.</param>
        ///// <returns>The newly-created message record.</returns>
        //[HttpPost("{connectionId}/messages")]
        //public async Task<IActionResult> SendMessage(int connectionId, [FromBody] ConnectionMessageRequest request)
        //{
        //    try
        //    {
        //        int senderUserId = User.GetUserId();
        //        var createdMessage = await _connectionManager.SendMessageAsync(connectionId, senderUserId, request);
        //        return Ok(createdMessage); // e.g. ConnectionMessageResponse
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        _logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
        //        return NotFound(ex.Message);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "Unauthorized access attempt to send a message.");
        //        return Forbid(ex.Message);
        //    }
        //    catch (BusinessException ex)
        //    {
        //        _logger.LogError(ex, "Error sending message.");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred while sending message.");
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}

        /// <summary>
        /// Get all trade proposals for a given connection.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>List of trade proposals for the connection.</returns>
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

        /// <summary>
        /// Create a new trade proposal for a given connection (the current user is the proposer).
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="request">Details about which plants are proposed from each side, etc.</param>
        /// <returns>The newly-created trade proposal record.</returns>
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

        /// <summary>
        /// Update the status of a trade proposal (e.g. accept, decline, complete).
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="proposalId"></param>
        /// <param name="request">Contains the new status (e.g. "Accepted", "Declined", "Completed").</param>
        /// <returns>No content on success.</returns>
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
    }
}
