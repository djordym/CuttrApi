using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class MessageRequest
    {
        public int MatchId { get; set; }
        public string MessageText { get; set; }
    }

}
