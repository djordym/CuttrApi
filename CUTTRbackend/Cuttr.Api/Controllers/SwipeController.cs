using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/swipes")]
    public class SwipeController : ControllerBase
    {
        private readonly ISwipeManager _swipeManager;
        private readonly ILogger<SwipeController> _logger;

        public SwipeController(ISwipeManager swipeManager, ILogger<SwipeController> logger)
        {
            _swipeManager = swipeManager;
            _logger = logger;
        }

        // Existing POST: api/swipes
        [HttpPost]
        public async Task<IActionResult> RecordSwipe([FromBody] List<SwipeRequest> requests)
        {
            try
            {
                var swipeResponses = await _swipeManager.RecordSwipesAsync(requests);
                return Ok(swipeResponses);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "One or more plants not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error recording swipes.");
                return BadRequest(ex.Message);
            }
        }

        // New GET: api/swipes/likable-plants
        [HttpGet("likable-plants")]
        [Authorize]
        public async Task<IActionResult> GetLikablePlants()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var likablePlants = await _swipeManager.GetLikablePlantsAsync(userId);
                return Ok(likablePlants);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving likable plants.");
                return BadRequest(ex.Message);
            }
        }
    }
}
