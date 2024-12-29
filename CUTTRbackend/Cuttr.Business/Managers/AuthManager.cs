using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthManager> _logger;

        public AuthManager(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            JwtTokenGenerator jwtTokenGenerator,
            ILogger<AuthManager> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
                    throw new AuthenticationException("Invalid email or password.");
                }

                var accessToken = _jwtTokenGenerator.GenerateToken(user, out int expiresIn);
                var refreshToken = GenerateRefreshToken();

                // Store hashed refresh token
                var tokenHash = HashToken(refreshToken);
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.UserId,
                    TokenHash = tokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _refreshTokenRepository.CreateRefreshTokenAsync(refreshTokenEntity);

                return new UserLoginResponse
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Tokens = new AuthTokenResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = expiresIn
                    }
                };
            }
            catch (AuthenticationException)
            {
                // We already log in the code above, so just re-throw
                throw;
            }
            catch (Exception ex)
            {
                // Catch all unexpected exceptions, log them and rethrow as a BusinessException or rethrow directly
                _logger.LogError(ex, "An error occurred while authenticating user with email: {Email}", request.Email);
                throw new BusinessException("An error occurred while authenticating the user.", ex);
            }
        }

        public async Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenHash = HashToken(refreshToken);
                var existingToken = await _refreshTokenRepository.GetRefreshTokenAsync(tokenHash);

                if (existingToken == null)
                {
                    _logger.LogWarning("Invalid or expired refresh token encountered.");
                    throw new AuthenticationException("Invalid or expired refresh token.");
                }

                // Revoke the old token
                await _refreshTokenRepository.RevokeRefreshTokenAsync(tokenHash);

                var user = await _userRepository.GetUserByIdAsync(existingToken.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found when refreshing token.", existingToken.UserId);
                    throw new NotFoundException("User not found.");
                }

                var accessToken = _jwtTokenGenerator.GenerateToken(user, out int expiresIn);
                var newRefreshToken = GenerateRefreshToken();
                var newTokenHash = HashToken(newRefreshToken);

                await _refreshTokenRepository.CreateRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.UserId,
                    TokenHash = newTokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                });

                return new AuthTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = expiresIn
                };
            }
            catch (AuthenticationException)
            {
                // We already log above or in the caller
                throw;
            }
            catch (NotFoundException)
            {
                // We log above
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the token.");
                throw new BusinessException("An error occurred while refreshing the token.", ex);
            }
        }

        public async Task LogoutUserAsync(int userId)
        {
            try
            {
                await _refreshTokenRepository.DeleteRefreshTokensForUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out user with ID {UserId}.", userId);
                throw new BusinessException("An error occurred while logging out.", ex);
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
