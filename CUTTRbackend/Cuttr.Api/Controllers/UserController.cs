using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;
using AuthenticationException = Cuttr.Business.Exceptions.AuthenticationException;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserManager userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // POST: api/users/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                var userResponse = await _userManager.RegisterUserAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { userId = userResponse.UserId }, userResponse);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error registering user.");
                return BadRequest(ex.Message);
            }
        }

        // POST: api/users/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var response = await _userManager.AuthenticateUserAsync(request);
                return Ok(response);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication failed.");
                return Unauthorized(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error during authentication.");
                return BadRequest(ex.Message);
            }
        }

        // GET: api/users/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var userResponse = await _userManager.GetUserByIdAsync(userId);
                return Ok(userResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/users/{userId}
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateRequest request)
        {
            try
            {
                var userResponse = await _userManager.UpdateUserAsync(userId, request);
                return Ok(userResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/users/{userId}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userManager.DeleteUserAsync(userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/users/{userId}/profile-picture
        [HttpPut("{userId}/profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfilePicture(int userId, [FromForm] UserProfileImageUpdateRequest request)
        {
            try
            {
                var userResponse = await _userManager.UpdateUserProfileImageAsync(userId, request);
                return Ok(userResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error updating profile picture for user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{userId}/location")]
        public async Task<IActionResult> UpdateLocation(int userId, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                await _userManager.UpdateUserLocationAsync(userId, request.Latitude, request.Longitude);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, $"User with ID {userId} not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, $"Error updating location for user with ID {userId}.");
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

    }
}
