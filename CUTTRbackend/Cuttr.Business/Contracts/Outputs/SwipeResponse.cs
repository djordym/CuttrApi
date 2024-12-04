using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class SwipeResponse
    {
        public bool IsMatch { get; set; }
        public MatchResponse Match { get; set; } // Included if IsMatch is true
    }
}
