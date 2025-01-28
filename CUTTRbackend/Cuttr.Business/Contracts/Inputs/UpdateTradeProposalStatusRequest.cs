using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class UpdateTradeProposalStatusRequest
    {
        public string NewStatus { get; set; }
        // e.g. "Accepted", "Declined", "Completed" 
        public string Reason { get; set; }
    }
}
