using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class SwipeResult
    {
        public bool IsMatch { get; set; }
        public Match Match { get; set; } // Included if IsMatch is true
    }
}
