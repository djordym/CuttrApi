using Cuttr.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class Plant
    {
        public int PlantId { get; set; }

        public int UserId { get; set; }

        public string SpeciesName { get; set; }

        public string? Description { get; set; }

        public PlantStage PlantStage { get; set; }
        public PlantCategory PlantCategory { get; set; }
        public WateringNeed WateringNeed { get; set; }
        public LightRequirement LightRequirement { get; set; }
        public Size Size { get; set; }
        public IndoorOutdoor IndoorOutdoor { get; set; }
        public PropagationEase PropagationEase { get; set; }
        public PetFriendly PetFriendly { get; set; }
        public List<Extras> Extras { get; set; }

        public string? ImageUrl { get; set; }

        // Reference to the owner
        public User User { get; set; }
    }
}
