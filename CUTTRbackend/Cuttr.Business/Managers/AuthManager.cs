using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                throw new AuthenticationException("Invalid email or password.");

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

        public async Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);
            var existingToken = await _refreshTokenRepository.GetRefreshTokenAsync(tokenHash);

            if (existingToken == null)
                throw new AuthenticationException("Invalid or expired refresh token.");

            await _refreshTokenRepository.RevokeRefreshTokenAsync(tokenHash);

            var user = await _userRepository.GetUserByIdAsync(existingToken.UserId);
            if (user == null)
                throw new NotFoundException("User not found.");

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

        public async Task LogoutUserAsync(int userId)
        {
            await _refreshTokenRepository.DeleteRefreshTokensForUserAsync(userId);
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
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
