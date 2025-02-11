using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Entities
{

    public class TradeProposalPlantEF
    {
        // Composite key (configured in the DbContext)
        public int TradeProposalId { get; set; }
        public virtual TradeProposalEF TradeProposal { get; set; }

        public int PlantId { get; set; }
        public virtual PlantEF Plant { get; set; }

        // If true, the plant was proposed by User1; if false, by User2.
        public bool IsProposedByUser1 { get; set; }
    }

}
