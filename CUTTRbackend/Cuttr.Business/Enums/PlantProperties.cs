using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Enums
{
    public enum PlantStage
    {
        Cutting,
        GrownPlantTree
    }

    public enum PlantCategory
    {
        Succulent,     // Thick, fleshy leaves, low water needs (e.g., Echeveria)
        Cactus,        // Spines/thorns, desert-adapted (subset of succulents but widely recognized)
        Fern,          // Moisture-loving, feather-like leaves
        Orchid,        // Epiphytic or terrestrial, often flowering
        Herb,          // Culinary or medicinal plants (e.g., Basil, Rosemary)
        Palm,          // Tropical trees with fronds, often indoor-friendly
        LeafyHouseplant, // General category for common indoor foliage varieties (e.g., Pothos)
        FloweringHouseplant, // General category for common indoor flowering varieties (e.g., African Violet)
        Other          // Catch-all for plants not fitting any category above
    }

    public enum WateringNeed
    {
        VeryLowWater,  // E.g., "Water every 2-3 weeks"
        LowWater,      // E.g., "Water when soil is fully dry"
        ModerateWater, // E.g., "Water about once a week or as topsoil dries"
        HighWater,      // E.g., "Keep soil consistently moist"
        VeryHighWater  // E.g., "Keep soil constantly wet"
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

    public enum IndoorOutdoor
    {
        Indoor,
        Outdoor,
        Both
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

    public enum Extras
    {
        Fragrant,
        Edible,
        Medicinal,
        AirPurifying,
        Decorative,
        Flowering,
        TropicalVibe,
        FoliageHeavy,
        DroughtTolerant,
        HumidityLoving,
        LowMaintenance,
        WinterHardy,
        BeginnerFriendly,
        Fruiting,
        PollinatorFriendly,
        FastGrowing,
        VariegatedFoliage,
        Climbing,
        GroundCover,
        Rare
    }

    
}
