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
        Task<PlantResponse> AddPlantAsync(PlantCreateRequest request);
        Task<PlantResponse> GetPlantByIdAsync(int plantId);
        Task<PlantResponse> UpdatePlantAsync(int plantId, PlantUpdateRequest request);
        Task DeletePlantAsync(int plantId);
        Task<IEnumerable<PlantResponse>> GetPlantsByUserIdAsync(int userId);
    }
}
