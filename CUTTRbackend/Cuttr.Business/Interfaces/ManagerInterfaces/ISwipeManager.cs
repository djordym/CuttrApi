using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface ISwipeManager
    {
        Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests, int userId);
        Task<List<PlantResponse>> GetLikablePlantsAsync(int userId);
    }
}
