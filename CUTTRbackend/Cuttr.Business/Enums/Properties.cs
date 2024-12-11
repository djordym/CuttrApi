using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Enums
{
    public enum CareRequirement
    {
        LowMaintenance,
        MediumMaintenance,
        HighMaintenance
    }

    public enum WateringFrequency
    {
        Rarely,
        Weekly,
        Daily
    }

    public enum LightRequirement
    {
        FullSun,
        PartialShade,
        Shade
    }

    public enum Size
    {
        Small,    
        Medium,   
        Large     
    }

    public enum ClimateSuitability
    {
        Tropical,
        Temperate,
        Arid,
        ColdResistant
    }

    public enum IndoorOutdoor
    {
        Indoor,
        Outdoor,
        Both
    }

    public enum BloomingSeason
    {
        Spring,
        Summer,
        Fall,
        YearRound
    }

    public enum SpecialFeature
    {
        Fragrant,
        Edible,
        Medicinal,
        AirPurifying,
        Decorative
    }

    public enum PropagationEase
    {
        Easy,
        Moderate,
        Difficult
    }

    public enum PetFriendly
    {
        Yes,
        No
    }

    public enum EcoFriendly
    {
        NativeSpecies,
        PollinatorFriendly
    }
}
