using Cuttr.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class PlantRequest
    {
        public string SpeciesName { get; set; }
        public string Description { get; set; }
        public PlantStage PlantStage { get; set; }
        public PlantCategory PlantCategory { get; set; }
        public WateringNeed WateringNeed { get; set; }
        public LightRequirement LightRequirement { get; set; }
        public Size? Size { get; set; }
        public IndoorOutdoor? IndoorOutdoor { get; set; }
        public PropagationEase? PropagationEase { get; set; }
        public PetFriendly? PetFriendly { get; set; }
        public List<Extras>? Extras { get; set; }
    }
}
