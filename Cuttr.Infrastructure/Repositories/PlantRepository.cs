using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Entities;
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
                var efPlant = BusinessToEFMapper.MapToPlantEFWithoutUser(plant);

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

        public async Task<IEnumerable<Plant>> GetTradablePlantsByUserIdAsync(int userId)
        {
            try
            {
                var efPlants = await _context.Plants.AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Where(p => p.IsTraded == false)
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

        public async Task<IEnumerable<Plant>> GetFilteredTradablePlantsAsync(
            int currentUserId,
            double originLat,
            double originLon,
            int radiusKm,
            UserPreferences preferences)
        {
            // Convert radius to meters
            double radiusMeters = radiusKm * 1000;
            var origin = new Point(originLon, originLat) { SRID = 4326 };

            // Base query: plant is tradable, not owned by the current user, and within radius.
            var query = _context.Plants
                                .AsNoTracking()
                                .Include(p => p.User)
                                .Where(p => !p.IsTraded &&
                                            p.UserId != currentUserId &&
                                            p.User.Location != null &&
                                            p.User.Location.Distance(origin) <= radiusMeters);

            // Convert enum lists to lists of strings and apply filters

            if (preferences.PreferedPlantStage?.Any() == true)
            {
                var preferredPlantStages = preferences.PreferedPlantStage
                                                     .Select(stage => stage.ToString())
                                                     .ToList();
                query = query.Where(p => preferredPlantStages.Contains(p.PlantStage));
            }

            if (preferences.PreferedPlantCategory?.Any() == true)
            {
                var preferredCategories = preferences.PreferedPlantCategory
                                                    .Select(cat => cat.ToString())
                                                    .ToList();
                query = query.Where(p => preferredCategories.Contains(p.PlantCategory));
            }

            if (preferences.PreferedWateringNeed?.Any() == true)
            {
                var preferredWateringNeeds = preferences.PreferedWateringNeed
                                                       .Select(need => need.ToString())
                                                       .ToList();
                query = query.Where(p => preferredWateringNeeds.Contains(p.WateringNeed));
            }

            if (preferences.PreferedLightRequirement?.Any() == true)
            {
                var preferredLightRequirements = preferences.PreferedLightRequirement
                                                            .Select(req => req.ToString())
                                                            .ToList();
                query = query.Where(p => preferredLightRequirements.Contains(p.LightRequirement));
            }

            if (preferences.PreferedSize?.Any() == true)
            {
                var preferredSizes = preferences.PreferedSize
                                            .Select(size => size.ToString())
                                            .ToList();
                query = query.Where(p => preferredSizes.Contains(p.Size));
            }

            if (preferences.PreferedIndoorOutdoor?.Any() == true)
            {
                var preferredIndoorOutdoor = preferences.PreferedIndoorOutdoor
                                                    .Select(io => io.ToString())
                                                    .ToList();
                query = query.Where(p => preferredIndoorOutdoor.Contains(p.IndoorOutdoor));
            }

            if (preferences.PreferedPropagationEase?.Any() == true)
            {
                var preferredPropagationEase = preferences.PreferedPropagationEase
                                                      .Select(ease => ease.ToString())
                                                      .ToList();
                query = query.Where(p => preferredPropagationEase.Contains(p.PropagationEase));
            }

            if (preferences.PreferedPetFriendly?.Any() == true)
            {
                var preferredPetFriendly = preferences.PreferedPetFriendly
                                                  .Select(pf => pf.ToString())
                                                  .ToList();
                query = query.Where(p => preferredPetFriendly.Contains(p.PetFriendly));
            }

            if (preferences.PreferedExtras?.Any() == true)
            {
                // Assuming Extras are stored in a way that allows a substring match.
                var preferredExtras = preferences.PreferedExtras
                                                  .Select(extra => extra.ToString())
                                                  .ToList();
                query = query.Where(p => p.Extras != null && preferredExtras.Any(extra => p.Extras.Contains(extra)));
            }

            // Randomize the results to ensure fairness.
            query = query.OrderBy(p => Guid.NewGuid());

            // Limit the number of candidates returned.
            var efPlants = await query.ToListAsync();
            return efPlants.Select(EFToBusinessMapper.MapToPlant).ToList();
        }


    }
}
