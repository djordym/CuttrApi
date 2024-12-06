using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface ISwipeRepository
    {
        Task AddSwipeAsync(Swipe swipe);
        Task<Swipe> GetSwipeAsync(int swiperPlantId, int swipedPlantId, bool isLike);
        Task<bool> HasSwipeAsync(int swiperPlantId, int swipedPlantId);

    }
}
