using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class PlantRequest
    {
        public int UserId { get; set; }
        public string SpeciesName { get; set; }
        public string CareRequirements { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
