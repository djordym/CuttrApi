using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IConnectionManager
    {
        Task<TradeProposalResponse> CreateTradeProposalAsync(int connectionId, int userId, TradeProposalRequest request);
        Task<ConnectionResponse> GetConnectionByIdAsync(int connectionId, int userId);
        Task<IEnumerable<ConnectionResponse>> GetConnectionsForUserAsync(int userId);
        Task<IEnumerable<TradeProposalResponse>> GetTradeProposalsAsync(int connectionId, int userId);
        Task UpdateTradeProposalStatusAsync(int connectionId, int proposalId, int userId, UpdateTradeProposalStatusRequest request);
        Task ConfirmTradeProposalCompletionAsync(int connectionId, int proposalId, int userId);
    }
}
