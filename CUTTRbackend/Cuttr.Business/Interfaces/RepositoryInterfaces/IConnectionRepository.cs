using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface IConnectionRepository
    {
        //Task<IEnumerable<Connection>> GetMatchesByUserIdAsync(int userId);
        //Task<Connection> GetMatchByIdAsync(int matchId);
        //// Other methods if necessary
        //Task<Connection> AddMatchAsync(Connection match);

        Task<Connection> GetConnectionByIdAsync(int connectionId);
        Task<IEnumerable<Connection>> GetConnectionsByUserIdAsync(int userId);
        Task<Connection> CreateConnectionAsync(Connection connection);
        Task<Connection> UpdateConnectionAsync(Connection connection);
        Task<Connection> GetConnectionByUsersAsync(int swiperUserId, int swipedUserId);
        Task<int> GetNumberOfMatchesAsync(int connectionId);
    }
}
