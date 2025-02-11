using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class UserLoginResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public AuthTokenResponse Tokens { get; set; }
    }
}
