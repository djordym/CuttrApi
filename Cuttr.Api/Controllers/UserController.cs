using Cuttr.Api.Common;
using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
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
        private readonly IAuthManager _authManager;

        public UserController(IUserManager userManager, ILogger<UserController> logger, IAuthManager authManager)
        {
            _userManager = userManager;
            _logger = logger;
            _authManager = authManager;
        }

        // POST: api/users/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                Console.WriteLine("Registering user...");
                var userResponse = await _userManager.RegisterUserAsync(request);
                //create loginresponse
                var loginresponse = await _authManager.AuthenticateUserAsync(new UserLoginRequest { Email = request.Email, Password = request.Password });
                return CreatedAtAction(nameof(GetUserById), new { userId = userResponse.UserId }, loginresponse);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error registering user.");
                return BadRequest(ex.InnerException.Message);
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

        [HttpGet("me")]
        public async Task<IActionResult> GetMeByToken()
        {
            try
            {
                var userId = User.GetUserId();
                var userResponse = await _userManager.GetUserByIdAsync(userId);
                return Ok(userResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error retrieving user.");
                return BadRequest(ex.Message);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
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

        [Authorize]
        [HttpPut("me/push-token")]
        public async Task<IActionResult> UpdatePushToken([FromBody] PushTokenUpdateRequest request)
        {
            try
            {
                int userId = User.GetUserId();
                await _userManager.UpdatePushTokenAsync(userId, request.ExpoPushToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating push token for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


        // temporary seed function that accepts list of registrationrequests
        [AllowAnonymous]
        [HttpPost("seed")]
        public async Task<IActionResult> SeedRegisterUsers([FromBody] List<UserRegistrationRequest> requests)
        {
            try
            {
                foreach (var request in requests)
                {
                    await _userManager.RegisterUserAsync(request);
                }
                return Ok();
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error seeding users.");
                return BadRequest(ex.Message);
            }
        }

        //accepts list of usersId's to update location from
        //to seed, change request object to have userId, latitude, and longitude
        [AllowAnonymous]
        [HttpPost("seed/locations")]
        public async Task<IActionResult> SeedUpdateLocations([FromBody] List<UpdateLocationRequest> requests)
        {
            try
            {
                foreach (var request in requests)
                {
                    await _userManager.UpdateUserLocationAsync(request.UserId, request.Latitude, request.Longitude);
                }
                return Ok();
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error seeding locations.");
                return BadRequest(ex.Message);
            }
        }
    }
}
