using Cuttr.Api.Common;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchManager _matchManager;
        private readonly ILogger<MatchController> _logger;

        public MatchController(IMatchManager matchManager, ILogger<MatchController> logger)
        {
            _matchManager = matchManager;
            _logger = logger;
        }

        // GET: api/matches/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMatches()
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                var matches = await _matchManager.GetMatchesByUserIdAsync(userId);
                return Ok(matches);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving matches for user.");
                return BadRequest(ex.Message);
            }
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving matches.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // GET: api/matches/{matchId}
        [AllowAnonymous]
        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatchById(int matchId)
        {
            try
            {
                var match = await _matchManager.GetMatchByIdAsync(matchId);
                return Ok(match);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Match with ID {matchId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving match with ID {matchId}.");
                return BadRequest(ex.Message);
            }
        }

        

    }
}
