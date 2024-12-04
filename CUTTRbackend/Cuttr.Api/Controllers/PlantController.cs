using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/plants")]
    public class PlantController : ControllerBase
    {
        private readonly IPlantManager _plantManager;
        private readonly ILogger<PlantController> _logger;

        public PlantController(IPlantManager plantManager, ILogger<PlantController> logger)
        {
            _plantManager = plantManager;
            _logger = logger;
        }

        // POST: api/plants
        [HttpPost]
        public async Task<IActionResult> AddPlant([FromBody] PlantRequest request)
        {
            try
            {
                var plantResponse = await _plantManager.AddPlantAsync(request);
                return CreatedAtAction(nameof(GetPlantById), new { plantId = plantResponse.PlantId }, plantResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error adding plant.");
                return BadRequest(ex.Message);
            }
        }

        // GET: api/plants/{plantId}
        [HttpGet("{plantId}")]
        public async Task<IActionResult> GetPlantById(int plantId)
        {
            try
            {
                var plantResponse = await _plantManager.GetPlantByIdAsync(plantId);
                return Ok(plantResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving plant with ID {plantId}.");
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/plants/{plantId}
        [HttpPut("{plantId}")]
        public async Task<IActionResult> UpdatePlant(int plantId, [FromBody] PlantUpdateRequest request)
        {
            try
            {
                var plantResponse = await _plantManager.UpdatePlantAsync(plantId, request);
                return Ok(plantResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error updating plant with ID {plantId}.");
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/plants/{plantId}
        [HttpDelete("{plantId}")]
        public async Task<IActionResult> DeletePlant(int plantId)
        {
            try
            {
                await _plantManager.DeletePlantAsync(plantId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error deleting plant with ID {plantId}.");
                return BadRequest(ex.Message);
            }
        }

        // GET: api/users/{userId}/plants
        [HttpGet("/api/users/{userId}/plants")]
        public async Task<IActionResult> GetPlantsByUserId(int userId)
        {
            try
            {
                var plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
                return Ok(plantResponses);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }
    }
}
