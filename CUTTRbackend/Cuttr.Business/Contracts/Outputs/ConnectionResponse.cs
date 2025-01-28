using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class ConnectionResponse
    {
        public int ConnectionId { get; set; }
        public UserResponse User1 { get; set; }
        public UserResponse User2 { get; set; }
        // Additional properties as needed
    }
}
