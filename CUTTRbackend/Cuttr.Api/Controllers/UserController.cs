using Cuttr.Api.Common;
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

        // PUT: api/me/users
        [HttpPut("me")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
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
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // DELETE: api/users/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
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
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while deleting the user with ID {userId}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

        }

        // PUT: api/users/me/profile-picture
        [HttpPut("me/profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UserProfileImageUpdateRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
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
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the profile picture.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPut("me/location")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
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
            catch (Business.Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the location.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

    }
}
