using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Entities
{
  
    public class UserPreferencesEF
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        //filters
        public int SearchRadius { get; set; } // e.g., in kilometers
        public List<string> PreferedPlantStage { get; set; }
        public List<string> PreferedPlantCategory { get; set; }
        public List<string> PreferedWateringNeed { get; set; }
        public List<string> PreferedLightRequirement { get; set; }
        public List<string> PreferedSize { get; set; }
        public List<string> PreferedIndoorOutdoor { get; set; }
        public List<string> PreferedPropagationEase { get; set; }
        public List<string> PreferedPetFriendly { get; set; }
        public List<string> PreferedExtras { get; set; }

        // Navigation property
        public virtual UserEF User { get; set; }
    }

}
