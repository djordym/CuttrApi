using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface IUserPreferencesRepository
    {
        Task<UserPreferences> GetUserPreferencesAsync(int userId);
        Task<UserPreferences> AddUserPreferencesAsync(UserPreferences preferences);
        Task UpdateUserPreferencesAsync(UserPreferences preferences);
    }
}
