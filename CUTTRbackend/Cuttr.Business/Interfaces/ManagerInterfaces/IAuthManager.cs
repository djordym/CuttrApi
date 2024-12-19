using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IAuthManager
    {
        Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
        Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutUserAsync(int userId);
    }
}
