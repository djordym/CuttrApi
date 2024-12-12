using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
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

        // GET: api/matches
        [HttpGet]
        public async Task<IActionResult> GetMatches()
        {
            try
            {
                // Assuming we have a way to get the authenticated user's ID
                int userId = GetAuthenticatedUserId();

                var matches = await _matchManager.GetMatchesByUserIdAsync(userId);
                return Ok(matches);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving matches for user.");
                return BadRequest(ex.Message);
            }
        }

        // GET: api/matches/{matchId}
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

        // Helper method to get authenticated user ID
        private int GetAuthenticatedUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

    }
}
