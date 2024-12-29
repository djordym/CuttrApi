using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
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
                _context.Entry(efPlant).State = EntityState.Detached;
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
                var efPlant = await _context.Plants.AsNoTracking()
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
                _context.Entry(efPlant).State = EntityState.Detached;
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
                _context.Entry(efPlant).State = EntityState.Detached;
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
                var efPlants = await _context.Plants.AsNoTracking()
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

        public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
        {
            try
            {
                var efPlants = await _context.Plants.AsNoTracking()
                    .Include(p => p.User)
                    .ToListAsync();

                return efPlants.Select(EFToBusinessMapper.MapToPlant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all plants.");
                throw new RepositoryException("An error occurred while retrieving all plants.", ex);
            }
        }

        public async Task<IEnumerable<Plant>> GetPlantsWithinRadiusAsync(double originLat, double originLon, double radiusKm)
        {
            // Convert radius to meters
            double radiusMeters = radiusKm * 1000;

            // Create an origin point
            var origin = new Point(originLon, originLat) { SRID = 4326 };

            // Query plants whose user's location is within the radius
            var efPlants = await _context.Plants
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.User.Location != null && p.User.Location.Distance(origin) <= radiusMeters)
                .ToListAsync();

            return efPlants.Select(EFToBusinessMapper.MapToPlant);
        }
    }
}
