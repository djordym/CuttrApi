using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Interfaces.Services;
using Cuttr.Business.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class PlantManager : IPlantManager
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PlantManager> _logger;
        private readonly IBlobStorageService _blobStorageService;

        private const string PlantImagesContainer = "plant-images";

        public PlantManager(IPlantRepository plantRepository, IUserRepository userRepository, ILogger<PlantManager> logger, IBlobStorageService blobStorageService)
        {
            _plantRepository = plantRepository;
            _userRepository = userRepository;
            _logger = logger;
            _blobStorageService = blobStorageService;
        }

        public async Task<PlantResponse> AddPlantAsync(PlantCreateRequest request, int userId)
        {
            try
            { 
                // Validate that the user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                string imageUrl = null;

                if (request.Image != null && request.Image.Length > 0)
                {
                    // Upload image to Azure Blob Storage in 'plant-images' container
                    imageUrl = await _blobStorageService.UploadFileAsync(request.Image, PlantImagesContainer);
                }

                var plant = ContractToBusinessMapper.MapToPlant(request.PlantDetails);
                plant.ImageUrl = imageUrl;

                var createdPlant = await _plantRepository.AddPlantAsync(plant);

                return BusinessToContractMapper.MapToPlantResponse(createdPlant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding plant.");
                throw new BusinessException("Error adding plant.", ex);
            }
        }

        public async Task<PlantResponse> GetPlantByIdAsync(int plantId)
        {
            try
            {
                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }
                return BusinessToContractMapper.MapToPlantResponse(plant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plant with ID {plantId}.");
                throw new BusinessException("Error retrieving plant.", ex);
            }
        }

        public async Task<PlantResponse> UpdatePlantAsync(int plantId,int userId, PlantRequest request)
        {
            try
            {

                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }

                // check if plant belong to user
                if (plant.UserId != userId)
                {
                    throw new BusinessException("Plant does not belong to user.");
                }

                // Update plant properties
                ContractToBusinessMapper.MapToPlantForUpdate(request, plant);

                await _plantRepository.UpdatePlantAsync(plant);

                return BusinessToContractMapper.MapToPlantResponse(plant);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating plant with ID {plantId}.");
                throw new BusinessException("Error updating plant.", ex);
            }
        }

        public async Task DeletePlantAsync(int plantId)
        {
            try
            {
                var plant = await _plantRepository.GetPlantByIdAsync(plantId);
                if (plant == null)
                {
                    throw new NotFoundException($"Plant with ID {plantId} not found.");
                }

                await _plantRepository.DeletePlantAsync(plantId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting plant with ID {plantId}.");
                throw new BusinessException("Error deleting plant.", ex);
            }
        }

        public async Task<IEnumerable<PlantResponse>> GetPlantsByUserIdAsync(int userId)
        {
            try
            {
                // Validate that the user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                var plants = await _plantRepository.GetPlantsByUserIdAsync(userId);

                return BusinessToContractMapper.MapToPlantResponse(plants);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
                throw new BusinessException("Error retrieving plants.", ex);
            }
        }
    }
}
