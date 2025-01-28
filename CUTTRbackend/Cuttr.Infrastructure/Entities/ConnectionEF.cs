using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;

namespace Cuttr.Infrastructure.Entities
{
    public class ConnectionEF : ICreatedAt
    {
        [Key]
        public int ConnectionId { get; set; }

        [Required]
        public int UserId1 { get; set; } // Owner of PlantId1

        [Required]
        public int UserId2 { get; set; } // Owner of PlantId2

        public bool isActive { get; set; }

        public DateTime CreatedAt { get; set; } // MatchedAt

        [ForeignKey("UserId1")]
        public virtual UserEF User1 { get; set; }

        [ForeignKey("UserId2")]
        public virtual UserEF User2 { get; set; }

        public virtual ICollection<MessageEF> Messages { get; set; }
        public List<TradeProposalEF> TradeProposals { get; set; }
    }
}
