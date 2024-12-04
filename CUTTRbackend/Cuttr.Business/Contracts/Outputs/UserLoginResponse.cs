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
        public string Token { get; set; }
        public User User { get; set; }
    }
}
