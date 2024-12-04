using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserRepository userRepository, ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(UserRegistrationRequest request)
        {
            try
            {
                // Check if user already exists
                if (await _userRepository.GetUserByEmailAsync(request.Email) != null)
                {
                    throw new BusinessException("Email already registered.");
                }

                // Hash the password (assuming a PasswordHasher utility)
                string passwordHash = PasswordHasher.HashPassword(request.Password);

                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Name = request.Name
                };

                var createdUser = await _userRepository.CreateUserAsync(user);
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user.");
                throw new BusinessException("Error registering user.", ex);
            }
        }

        public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    throw new AuthenticationException("Invalid email or password.");
                }

                // Generate JWT token (assuming a JwtTokenGenerator utility)
                string token = JwtTokenGenerator.GenerateToken(user);

                return new UserLoginResponse
                {
                    Token = token,
                    User = user
                };
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user.");
                throw new BusinessException("Error authenticating user.", ex);
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                return user;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {userId}.");
                throw new BusinessException("Error retrieving user.", ex);
            }
        }

        public async Task<User> UpdateUserAsync(int userId, UserUpdateRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                // Update user properties
                user.Name = request.Name ?? user.Name;
                user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
                user.Bio = request.Bio ?? user.Bio;
                user.LocationLatitude = request.LocationLatitude ?? user.LocationLatitude;
                user.LocationLongitude = request.LocationLongitude ?? user.LocationLongitude;

                await _userRepository.UpdateUserAsync(user);
                return user;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {userId}.");
                throw new BusinessException("Error updating user.", ex);
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                await _userRepository.DeleteUserAsync(userId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {userId}.");
                throw new BusinessException("Error deleting user.", ex);
            }
        }
    }
}
