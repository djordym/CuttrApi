using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Enums
{
    public enum PlantStage
    {
        Seedling,
        Cutting,
        Mature
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
        AquaticPlant,
        ClimbingPlant,
        Tree,          // Woody plants with a single trunk
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
        PartialSun,
        BrightIndirectLight,
        LowLight
    }

    public enum Size
    {
        SmallSize,    
        MediumSize,   
        LargeSize     
    }

    public enum IndoorOutdoor
    {
        Indoor,
        Outdoor,
        IndoorAndOutdoor
    }

    public enum PropagationEase
    {
        EasyPropagation,
        ModeratePropagation,
        DifficultPropagation
    }

    public enum PetFriendly
    {
        PetFriendly,
        NotPetFriendly
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
