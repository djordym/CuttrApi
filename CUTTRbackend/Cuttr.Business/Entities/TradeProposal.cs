using Cuttr.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class TradeProposal
    {
        public int TradeProposalId { get; set; }
        public int ConnectionId { get; set; }

        public List<int> PlantIdsProposedByUser1 { get; set; }
        public List<int> PlantIdsProposedByUser2 { get; set; }

        public List<Plant> PlantsProposedByUser1 { get; set; }
        public List<Plant> PlantsProposedByUser2 { get; set; }

        public TradeProposalStatus TradeProposalStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeclinedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Connection Connection { get; set; }

        public int ProposalOwnerUserId { get; set; }
        public bool OwnerCompletionConfirmed { get; set; }
        public bool ResponderCompletionConfirmed { get; set; }

    }
}
