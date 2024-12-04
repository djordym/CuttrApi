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
    public interface IUserManager
    {
        Task<User> RegisterUserAsync(UserRegistrationRequest request);
        Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> UpdateUserAsync(int userId, UserUpdateRequest request);
        Task DeleteUserAsync(int userId);
    }
}
