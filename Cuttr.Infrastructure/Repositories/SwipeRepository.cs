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
    public class SwipeRepository : ISwipeRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<SwipeRepository> _logger;

        public SwipeRepository(CuttrDbContext context, ILogger<SwipeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddSwipeAsync(Swipe swipe)
        {
            try
            {
                var efSwipe = BusinessToEFMapper.MapToSwipeEF(swipe);
                efSwipe.SwipeId = 0; // Ensure the ID is unset for new entities

                await _context.Swipes.AddAsync(efSwipe);
                await _context.SaveChangesAsync();
                _context.Entry(efSwipe).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a swipe.");
                throw new RepositoryException("An error occurred while adding a swipe.", ex);
            }
        }

        public async Task<Swipe> GetSwipeAsync(int swiperPlantId, int swipedPlantId, bool isLike)
        {
            try
            {
                var efSwipe = await _context.Swipes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                    s.SwiperPlantId == swiperPlantId &&
                    s.SwipedPlantId == swipedPlantId &&
                    s.IsLike == isLike);

                return EFToBusinessMapper.MapToSwipe(efSwipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a swipe.");
                throw new RepositoryException("An error occurred while retrieving a swipe.", ex);
            }
        }

        public async Task<bool> HasSwipeAsync(int swiperPlantId, int swipedPlantId)
        {
            try
            {
                var exists = await _context.Swipes.AsNoTracking().AnyAsync(s =>
                    s.SwiperPlantId == swiperPlantId &&
                    s.SwipedPlantId == swipedPlantId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking swipe existence.");
                throw new RepositoryException("An error occurred while checking swipe existence.", ex);
            }
        }

        public async Task<Swipe> GetSwipeForPairAsync(int swiperPlantId, int swipedPlantId)
        {
            try
            {
                var efSwipe = await _context.Swipes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.SwiperPlantId == swiperPlantId &&
                        s.SwipedPlantId == swipedPlantId);

                return EFToBusinessMapper.MapToSwipe(efSwipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a swipe for pair.");
                throw new RepositoryException("An error occurred while retrieving a swipe for pair.", ex);
            }
        }

        public async Task UpdateSwipeAsync(Swipe swipe)
        {
            try
            {
                var efSwipe = BusinessToEFMapper.MapToSwipeEF(swipe);
                _context.Swipes.Update(efSwipe);
                await _context.SaveChangesAsync();
                _context.Entry(efSwipe).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a swipe.");
                throw new RepositoryException("An error occurred while updating a swipe.", ex);
            }
        }

        public async Task<IEnumerable<Plant>> GetTradableLikedPlantsBySwiperAsync(int swiperUserId, int swipedPlantOwnerUserId)
        {
            try
            {
                var likedPlants = await _context.Swipes
                        .AsNoTracking()
                        .Where(s =>
                            s.SwiperPlant.User.UserId == swiperUserId &&
                            s.SwipedPlant.User.UserId == swipedPlantOwnerUserId &&
                            s.IsLike)
                        .GroupBy(s => s.SwipedPlant.PlantId)
                        .Select(g => g.First().SwipedPlant)
                        .ToListAsync();


                return likedPlants.Select(EFToBusinessMapper.MapToPlantWithoutUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving liked plants.");
                throw new RepositoryException("An error occurred while retrieving liked plants.", ex);
            }
        }

        public async Task<IEnumerable<Swipe>> GetSwipesForUserPlantsAsync(List<int> userPlantIds, List<int> candidatePlantIds)
        {
            try
            {
                var swipes = await _context.Swipes
                    .AsNoTracking()
                    .Where(s => userPlantIds.Contains(s.SwiperPlantId) && candidatePlantIds.Contains(s.SwipedPlantId))
                    .ToListAsync();

                return swipes.Select(EFToBusinessMapper.MapToSwipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving swipes for user plants.");
                throw new RepositoryException("An error occurred while retrieving swipes for user plants.", ex);
            }
        }

    }
}
