using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;

namespace Cuttr.Business.Mappers
{
    public static class ContractToBusinessMapper
    {
        public static User MapToUser(UserRegistrationRequest request)
        {
            if (request == null)
                return null;

            return new User
            {
                Email = request.Email,
                PasswordHash = request.Password, // Will be hashed in UserManager
                Name = request.Name
            };
        }

        // Map UserUpdateRequest to existing User
        public static void MapToUser(UserUpdateRequest request, User user)
        {
            if (request == null || user == null)
                return;

            user.Name = request.Name ?? user.Name;
            user.Bio = request.Bio ?? user.Bio;
        }

        // Map PlantRequest to Plant
        public static Plant MapToPlant(PlantRequest request)
        {
            if (request == null)
                return null;

            return new Plant
            {
                SpeciesName = request.SpeciesName,
                Description = request.Description,
                PlantStage = request.PlantStage,
                PlantCategory = request.PlantCategory,
                WateringNeed = request.WateringNeed,
                LightRequirement = request.LightRequirement,
                Size = request.Size,
                IndoorOutdoor = request.IndoorOutdoor,
                PropagationEase = request.PropagationEase,
                PetFriendly = request.PetFriendly,
                Extras = request.Extras       
            };
        }

        public static void MapToPlantForUpdate(PlantRequest request, Plant plant)
        {
            if (request == null || plant == null)
                return;

            plant.SpeciesName = request.SpeciesName ?? plant.SpeciesName;
            plant.Description = request.Description ?? plant.Description;
            plant.PlantStage = request.PlantStage;
            plant.PlantCategory = request.PlantCategory;
            plant.WateringNeed = request.WateringNeed;
            plant.LightRequirement = request.LightRequirement;
            plant.Size = request.Size;
            plant.IndoorOutdoor = request.IndoorOutdoor;
            plant.PropagationEase = request.PropagationEase;
            plant.PetFriendly = request.PetFriendly;
            plant.Extras = request.Extras;
        }

        public static Swipe MapToSwipe(SwipeRequest request)
        {
            if (request == null)
                return null;

            return new Swipe
            {
                SwiperPlantId = request.SwiperPlantId,
                SwipedPlantId = request.SwipedPlantId,
                IsLike = request.IsLike
            };
        }

        public static Message MapToMessage(MessageRequest request, int senderUserId)
        {
            if (request == null)
                return null;

            return new Message
            {
                MatchId = request.MatchId,
                SenderUserId = senderUserId,
                MessageText = request.MessageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
        }

        public static UserPreferences MapToUserPreferences(UserPreferencesRequest request)
        {
            if (request == null)
                return null;

            return new UserPreferences
            {
                SearchRadius = request.SearchRadius,
                PreferedPlantStage = request.PreferedPlantStage,
                PreferedPlantCategory = request.PreferedPlantCategory,
                PreferedWateringNeed = request.PreferedWateringNeed,
                PreferedLightRequirement = request.PreferedLightRequirement,
                PreferedSize = request.PreferedSize,
                PreferedIndoorOutdoor = request.PreferedIndoorOutdoor,
                PreferedPropagationEase = request.PreferedPropagationEase,
                PreferedPetFriendly = request.PreferedPetFriendly,
                PreferedExtras = request.PreferedExtras
            };
        }
    }
}
