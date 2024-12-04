using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class PlantResponse
    {
        public int PlantId { get; set; }
        public int UserId { get; set; }
        public string SpeciesName { get; set; }
        public string CareRequirements { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        // Exclude any internal fields
    }
}
