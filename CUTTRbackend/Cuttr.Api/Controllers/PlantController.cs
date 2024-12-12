using Cuttr.Api.Common;
using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UnauthorizedAccessException = Cuttr.Business.Exceptions.UnauthorizedAccessException;

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
        [HttpPost("me")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPlant([FromForm] PlantCreateRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                var plantResponse = await _plantManager.AddPlantAsync(request, userId);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding the plant.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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
        [HttpPut("me/{plantId}")]
        public async Task<IActionResult> UpdatePlant(int plantId, [FromBody] PlantRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                var plantResponse = await _plantManager.UpdatePlantAsync(plantId, userId, request);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the plant.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // DELETE: api/plants/{plantId}
        [HttpDelete("me/{plantId}")]
        public async Task<IActionResult> DeletePlant(int plantId)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                await _plantManager.DeletePlantAsync(plantId, userId);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting the plant.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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

        [HttpGet("/api/users/me/plants")]
        public async Task<IActionResult> GetPlantsOfUser()
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                var plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
                return Ok(plantResponses);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving the plants.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
