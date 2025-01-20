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
        Succulent,     
        Cactus,       
        Fern,          
        Orchid,        
        Herb,          
        Palm,          
        LeafyHouseplant,
        AquaticPlant,
        ClimbingPlant,
        Tree,          
        Other          
    }

    public enum WateringNeed
    {
        VeryLowWater,  
        LowWater,      
        ModerateWater, 
        HighWater,     
        VeryHighWater  
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
