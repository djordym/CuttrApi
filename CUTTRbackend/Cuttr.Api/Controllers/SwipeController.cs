using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
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

        // POST: api/swipes
        [HttpPost]
        public async Task<IActionResult> RecordSwipe([FromBody] SwipeRequest request)
        {
            try
            {
                var swipeResponse = await _swipeManager.RecordSwipeAsync(request);
                return Ok(swipeResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Plant not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error recording swipe.");
                return BadRequest(ex.Message);
            }
        }
    }
}
