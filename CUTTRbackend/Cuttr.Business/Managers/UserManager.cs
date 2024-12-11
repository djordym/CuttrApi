using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Interfaces.Services;
using Cuttr.Business.Mappers;
using Cuttr.Business.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using AuthenticationException = Cuttr.Business.Exceptions.AuthenticationException;

namespace Cuttr.Business.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserManager> _logger;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IBlobStorageService _blobStorageService;

        private const string ProfileImagesContainer = "profile-images";

        public UserManager(IUserRepository userRepository, ILogger<UserManager> logger, JwtTokenGenerator jwtTokenGenerator, IBlobStorageService blobStorageService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _jwtTokenGenerator = jwtTokenGenerator;
            _blobStorageService = blobStorageService;
        }

        public async Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request)
        {
            try
            {
                // Check if user already exists
                if (await _userRepository.GetUserByEmailAsync(request.Email) != null)
                {
                    throw new BusinessException("Email already registered.");
                }

                // Map to User entity
                var user = ContractToBusinessMapper.MapToUser(request);

                // Hash the password
                user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);

                var createdUser = await _userRepository.CreateUserAsync(user);

                // Map to UserResponse
                return BusinessToContractMapper.MapToUserResponse(createdUser);
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

                // Generate JWT token
                string token = _jwtTokenGenerator.GenerateToken(user);

                // Map to UserLoginResponse
                return BusinessToContractMapper.MapToUserLoginResponse(user, token);
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

        public async Task<UserResponse> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                return BusinessToContractMapper.MapToUserResponse(user);
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

        public async Task<UserResponse> UpdateUserAsync(int userId, UserUpdateRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                // Update user properties
                ContractToBusinessMapper.MapToUser(request, user);

                await _userRepository.UpdateUserAsync(user);

                return BusinessToContractMapper.MapToUserResponse(user);
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

        public async Task<UserResponse> UpdateUserProfileImageAsync(int userId, UserProfileImageUpdateRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                string imageUrl = null;

                if (request.Image != null && request.Image.Length > 0)
                {
                    // Upload image to Azure Blob Storage in 'profile-images' container
                    imageUrl = await _blobStorageService.UploadFileAsync(request.Image, ProfileImagesContainer);

                    // Optionally, delete the old profile picture from Blob Storage if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        await _blobStorageService.DeleteFileAsync(user.ProfilePictureUrl, ProfileImagesContainer);
                    }

                    user.ProfilePictureUrl = imageUrl;
                }

                await _userRepository.UpdateUserAsync(user);

                return BusinessToContractMapper.MapToUserResponse(user);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile picture for user with ID {userId}.");
                throw new BusinessException("Error updating profile picture.", ex);
            }
        }
    }
}
