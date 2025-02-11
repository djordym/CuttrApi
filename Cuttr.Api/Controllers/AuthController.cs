using Cuttr.Api.Common;
using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using AuthenticationException = Cuttr.Business.Exceptions.AuthenticationException;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthManager authManager, ILogger<AuthController> logger)
        {
            _authManager = authManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var response = await _authManager.AuthenticateUserAsync(request);
                return Ok(response);
            }
            catch (AuthenticationException ex)
            {
                // Usually for invalid credentials or expired tokens, you'd return 401
                _logger.LogWarning(ex, "Authentication failed for email: {Email}", request.Email);
                return Unauthorized(ex.Message);
            }
            catch (BusinessException ex)
            {
                // This handles any known business-related errors
                _logger.LogError(ex, "Business error occurred while logging in user with email: {Email}", request.Email);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Generic fallback for unexpected exceptions
                _logger.LogError(ex, "An unexpected error occurred while logging in user with email: {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            int userId = 0;
            try
            {
                userId = User.GetUserId();
                await _authManager.LogoutUserAsync(userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found when attempting to log out.", userId);
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Business error occurred while logging out user with ID {UserId}.", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while logging out user with ID {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authManager.RefreshTokenAsync(request.RefreshToken);
                return Ok(response);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Invalid or expired refresh token used.");
                return Unauthorized(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Business error occurred while refreshing the token.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while refreshing the token.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
