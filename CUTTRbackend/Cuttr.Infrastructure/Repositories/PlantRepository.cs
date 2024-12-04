using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<PlantRepository> _logger;

        public PlantRepository(CuttrDbContext context, ILogger<PlantRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Plant> AddPlantAsync(Plant plant)
        {
            try
            {
                var efPlant = BusinessToEFMapper.MapToPlantEF(plant);
                efPlant.PlantId = 0; // Ensure the ID is unset for new entities

                await _context.Plants.AddAsync(efPlant);
                await _context.SaveChangesAsync();

                return EFToBusinessMapper.MapToPlant(efPlant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a plant.");
                throw new RepositoryException("An error occurred while adding a plant.", ex);
            }
        }

        public async Task<Plant> GetPlantByIdAsync(int plantId)
        {
            try
            {
                var efPlant = await _context.Plants
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PlantId == plantId);

                if (efPlant == null)
                {
                    _logger.LogWarning($"Plant with ID {plantId} not found.");
                    return null;
                }

                return EFToBusinessMapper.MapToPlant(efPlant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving plant with ID {plantId}.");
                throw new RepositoryException("An error occurred while retrieving plant.", ex);
            }
        }

        public async Task UpdatePlantAsync(Plant plant)
        {
            try
            {
                var efPlant = BusinessToEFMapper.MapToPlantEF(plant);

                _context.Plants.Update(efPlant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"A concurrency error occurred while updating plant with ID {plant.PlantId}.");
                throw new RepositoryException("A concurrency error occurred while updating plant.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating plant with ID {plant.PlantId}.");
                throw new RepositoryException("An error occurred while updating plant.", ex);
            }
        }

        public async Task DeletePlantAsync(int plantId)
        {
            try
            {
                var efPlant = await _context.Plants.FindAsync(plantId);
                if (efPlant == null)
                {
                    _logger.LogWarning($"Plant with ID {plantId} not found.");
                    return;
                }

                _context.Plants.Remove(efPlant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"A database error occurred while deleting plant with ID {plantId}.");
                throw new RepositoryException("A database error occurred while deleting plant.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting plant with ID {plantId}.");
                throw new RepositoryException("An error occurred while deleting plant.", ex);
            }
        }

        public async Task<IEnumerable<Plant>> GetPlantsByUserIdAsync(int userId)
        {
            try
            {
                var efPlants = await _context.Plants
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .ToListAsync();

                return efPlants.Select(EFToBusinessMapper.MapToPlant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving plants for user with ID {userId}.");
                throw new RepositoryException("An error occurred while retrieving plants.", ex);
            }
        }
    }
}
