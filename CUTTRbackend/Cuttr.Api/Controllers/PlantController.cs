using Cuttr.Api.Common;
using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public async Task<ActionResult<PlantResponse>> AddPlant([FromForm] PlantCreateRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                PlantResponse plantResponse = await _plantManager.AddPlantAsync(request, userId);
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
        public async Task<ActionResult<PlantResponse>> GetPlantById(int plantId)
        {
            try
            {
                PlantResponse plantResponse = await _plantManager.GetPlantByIdAsync(plantId);
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
        public async Task<ActionResult<PlantResponse>> UpdatePlant(int plantId, [FromBody] PlantRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                PlantResponse plantResponse = await _plantManager.UpdatePlantAsync(plantId, userId, request);
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
        [HttpGet("users/{userId}")]
        public async Task<ActionResult<List<PlantResponse>>> GetPlantsByUserId(int userId)
        {
            try
            {
                List<PlantResponse> plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
                return Ok(plantResponses);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/me")]
        public async Task<ActionResult<List<PlantResponse>>> GetPlantsOfUser()
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                List<PlantResponse> plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
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

        // New GET: api/swipes/likable-plants
        [HttpGet("likable")]
        public async Task<ActionResult<List<PlantResponse>>> GetLikablePlants(int maxCount = 10)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                List<PlantResponse> likablePlants = await _plantManager.GetLikablePlantsAsync(userId, maxCount);
                return Ok(likablePlants);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving likable plants.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("liked-by-me/from/{userAId}")]
        public async Task<ActionResult<List<PlantResponse>>> GetPlantsLikedByMeFromUser(int userAId)
        {
            try
            {
                int currentUserId = User.GetUserId();
                List<PlantResponse> likedPlants = await _plantManager.GetPlantsLikedByMeFromUserAsync(userAId, currentUserId);
                return Ok(likedPlants);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userAId} not found.");
                return NotFound(new { error = ex.Message });
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving liked plants.");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving liked plants.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        [HttpGet("liked-by/{userAId}/from-me")]
        public async Task<ActionResult<List<PlantResponse>>> GetPlantsLikedByUserFromMe(int userAId)
        {
            try
            {
                int currentUserId = User.GetUserId();
                List<PlantResponse> plantsLikedByUser = await _plantManager.GetPlantsLikedByUserFromMeAsync(userAId, currentUserId);
                return Ok(plantsLikedByUser);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userAId} not found.");
                return NotFound(new { error = ex.Message });
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving plants liked by user.");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving plants liked by user.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("mark-as-traded/{plantId}")]
        public async Task<IActionResult> MarkPlantAsTraded(int plantId)
        {
            try
            {
                int userId = User.GetUserId();
                await _plantManager.MarkPlantAsTradedAsync(plantId, userId);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error marking plant with ID {plantId} as traded.");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while marking the plant as traded.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }






        //seed plants
        [AllowAnonymous]
        [HttpPost("seed")]
        public async Task<IActionResult> SeedPlants([FromBody] List<SeedPlantRequest> request)
        {
            try
            {
                foreach (var plant in request)
                {
                    await _plantManager.SeedPlantAsync(plant);
                }
                return Ok();
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error seeding plants.");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while seeding plants.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


    }
}
