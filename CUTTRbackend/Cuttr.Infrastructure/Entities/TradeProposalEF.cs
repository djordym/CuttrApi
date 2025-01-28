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

        /// <summary>
        /// A string (e.g. JSON) containing the plant IDs proposed by User1.
        /// Example: "[1,2]" or "1,2"
        /// </summary>
        public string PlantIdsProposedByUser1 { get; set; }

        /// <summary>
        /// A string (e.g. JSON) containing the plant IDs proposed by User2.
        /// Example: "[5,9]" or "5,9"
        /// </summary>
        public string PlantIdsProposedByUser2 { get; set; }

        /// <summary>
        /// Non-mapped collections of PlantEF that you could populate
        /// by parsing the above string fields and fetching the corresponding plants.
        /// EF will NOT automatically load these because they're [NotMapped].
        /// </summary>
        [NotMapped]
        public List<PlantEF> ItemsProposedByUser1 { get; set; } = new();

        [NotMapped]
        public List<PlantEF> ItemsProposedByUser2 { get; set; } = new();

        /// <summary>
        /// Tracks the status of the proposal. Could be "Proposed", "Accepted", "Declined", or "Completed", etc.
        /// </summary>
        public string TradeProposalStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeclinedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [ForeignKey("ConnectionId")]
        public ConnectionEF Connection { get; set; }
    }
}
