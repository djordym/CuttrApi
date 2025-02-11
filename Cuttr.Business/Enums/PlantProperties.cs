using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cuttr.Business.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PlantStage
    {
        Seedling,
        Cutting,
        Mature
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WateringNeed
    {
        VeryLowWater,  
        LowWater,      
        ModerateWater, 
        HighWater,     
        VeryHighWater  
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LightRequirement
    {
        FullSun,
        PartialSun,
        BrightIndirectLight,
        LowLight
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Size
    {
        SmallSize,    
        MediumSize,   
        LargeSize     
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IndoorOutdoor
    {
        Indoor,
        Outdoor,
        IndoorAndOutdoor
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PropagationEase
    {
        EasyPropagation,
        ModeratePropagation,
        DifficultPropagation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PetFriendly
    {
        PetFriendly,
        NotPetFriendly
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
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
