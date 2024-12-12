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
        public string PreferedPlantStage { get; set; }
        public string PreferedPlantCategory { get; set; }
        public string PreferedWateringNeed { get; set; }
        public string PreferedLightRequirement { get; set; }
        public string PreferedSize { get; set; }
        public string PreferedIndoorOutdoor { get; set; }
        public string PreferedPropagationEase { get; set; }
        public string PreferedPetFriendly { get; set; }
        public string PreferedExtras { get; set; }

        // Navigation property
        public virtual UserEF User { get; set; }
    }

}
