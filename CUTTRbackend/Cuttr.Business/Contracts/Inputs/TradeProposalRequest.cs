using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class TradeProposalRequest
    {
        // E.g. if user picks 1 or multiple plants:
        public List<int> UserPlantIds { get; set; }    // The plants the current user is offering
        public List<int> OtherPlantIds { get; set; }   // The plants they want from the other user
        public string AdditionalNotes { get; set; }     // Possibly text about the offer
    }
}
