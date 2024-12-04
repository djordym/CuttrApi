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
                var efSwipe = await _context.Swipes.FirstOrDefaultAsync(s =>
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
    }
}
