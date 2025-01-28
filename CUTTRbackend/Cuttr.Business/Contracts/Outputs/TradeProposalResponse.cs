using Cuttr.Business.Entities;
using Cuttr.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class TradeProposalResponse
    {
        public int TradeProposalId { get; set; }
        public int ConnectionId { get; set; }

        // E.g. JSON or child rows that reference which plants 
        // are being traded on each side
        public List<PlantResponse> ItemsProposedByUser1 { get; set; }
        public List<PlantResponse> ItemsProposedByUser2 { get; set; }

        // If you keep it simpler: just store a single "PlantId from user1" 
        // and "PlantId from user2" if it's strictly 1-1 trades.

        public TradeProposalStatus TradeProposalStatus { get; set; }
        // e.g. { Proposed = 1, Accepted, Declined, Completed }

        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeclinedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public ConnectionResponse Connection { get; set; }
    }
}
