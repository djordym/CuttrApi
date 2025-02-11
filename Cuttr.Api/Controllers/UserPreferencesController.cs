using Cuttr.Api.Common;
using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuttr.Api.Controllers
{

    [ApiController]
    [Route("api/userpreferences")]
    [Authorize]
    public class UserPreferencesController : ControllerBase
    {
        private readonly IUserPreferencesManager _userPreferencesManager;
        private readonly ILogger<UserPreferencesController> _logger;

        public UserPreferencesController(IUserPreferencesManager userPreferencesManager, ILogger<UserPreferencesController> logger)
        {
            _userPreferencesManager = userPreferencesManager;
            _logger = logger;
        }

        // GET: api/userpreferences
        [HttpGet]
        public async Task<IActionResult> GetUserPreferences()
        {
            try
            {
                int userId = User.GetUserId();

                var preferences = await _userPreferencesManager.GetUserPreferencesAsync(userId);
                return Ok(preferences);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User preferences for user ID {User.GetUserId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving user preferences.");
                return BadRequest(ex.Message);
            }
        }

        // POST: api/userpreferences
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateUserPreferences([FromBody] UserPreferencesRequest request)
        {
            try
            {
                int userId = User.GetUserId();

                var preferences = await _userPreferencesManager.CreateOrUpdateUserPreferencesAsync(userId, request);
                return Ok(preferences);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error creating or updating user preferences.");
                return BadRequest(ex.Message);
            }
        }
    }
}

