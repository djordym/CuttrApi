using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IPlantManager
    {
        Task<PlantResponse> AddPlantAsync(PlantCreateRequest request, int userId);
        Task<PlantResponse> GetPlantByIdAsync(int plantId);
        Task<PlantResponse> UpdatePlantAsync(int plantId, int userId, PlantRequest request);
        Task DeletePlantAsync(int plantId, int userId);
        Task<IEnumerable<PlantResponse>> GetPlantsByUserIdAsync(int userId);
        Task SeedPlantAsync(SeedPlantRequest plant);
    }
}
