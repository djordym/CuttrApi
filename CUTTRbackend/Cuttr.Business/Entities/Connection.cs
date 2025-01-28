using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class Connection
    {
        public int ConnectionId { get; set; }

        public int UserId1 { get; set; }

        public int UserId2 { get; set; }

        public DateTime CreatedAt { get; set; }

        // References to plants and users
        public User User1 { get; set; }

        public User User2 { get; set; }

        public bool IsActive { get; set; }

        // Messages in the match
        public List<Message> Messages { get; set; }

        public List<TradeProposal> TradeProposals { get; set; }
    }
}
