using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cuttr.Infrastructure.Entities
{
    public class TradeProposalEF
    {
        [Key]
        public int TradeProposalId { get; set; }

        public int ConnectionId { get; set; }
        public int ProposalOwnerUserId { get; set; }
        public bool OwnerCompletionConfirmed { get; set; }
        public bool ResponderCompletionConfirmed { get; set; }

        public string TradeProposalStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeclinedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [ForeignKey("ConnectionId")]
        public virtual ConnectionEF Connection { get; set; }

        // Navigation property for the join table that links plants to this proposal.
        public virtual ICollection<TradeProposalPlantEF> TradeProposalPlants { get; set; } = new List<TradeProposalPlantEF>();
    }
}
