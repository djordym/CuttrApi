using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface IMatchRepository
    {
        Task<IEnumerable<Match>> GetMatchesByConnectionIdAsync(int connectionId);
        Task<Match> GetMatchByIdAsync(int matchId);
        // Other methods if necessary
        Task<Match> AddMatchAsync(Match match);
    }
}
