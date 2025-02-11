using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IMatchManager
    {
        Task<IEnumerable<MatchResponse>> GetMatchesByConnectionIdAsync(int userId);
        Task<MatchResponse> GetMatchByIdAsync(int matchId);
    }
}
