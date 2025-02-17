﻿using Cuttr.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class UserPreferencesResponse
    {
        public int UserId { get; set; }
        public double SearchRadius { get; set; }
        public List<PlantStage> PreferedPlantStage { get; set; }
        public List<PlantCategory> PreferedPlantCategory { get; set; }
        public List<WateringNeed> PreferedWateringNeed { get; set; }
        public List<LightRequirement> PreferedLightRequirement { get; set; }
        public List<Size> PreferedSize { get; set; }
        public List<IndoorOutdoor> PreferedIndoorOutdoor { get; set; }
        public List<PropagationEase> PreferedPropagationEase { get; set; }
        public List<PetFriendly> PreferedPetFriendly { get; set; }
        public List<Extras> PreferedExtras { get; set; }
    }

}
