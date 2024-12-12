using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageManager _messageManager;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageManager messageManager, ILogger<MessageController> logger)
        {
            _messageManager = messageManager;
            _logger = logger;
        }

        // POST: api/messages
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            try
            {
                int senderUserId = GetAuthenticatedUserId();

                var messageResponse = await _messageManager.SendMessageAsync(request, senderUserId);
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
        }

        // GET: api/matches/{matchId}/messages
        [HttpGet("/api/matches/{matchId}/messages")]
        public async Task<IActionResult> GetMessages(int matchId)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var messages = await _messageManager.GetMessagesByMatchIdAsync(matchId, userId);
                return Ok(messages);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Match with ID {matchId} not found.");
                return NotFound(ex.Message);
            }
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to messages.");
                return Forbid(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for match with ID {matchId}.");
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

    }
}
