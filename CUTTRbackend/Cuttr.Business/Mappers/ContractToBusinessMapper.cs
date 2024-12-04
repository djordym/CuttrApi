using Cuttr.Business.Contracts.Inputs;
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
            user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
            user.Bio = request.Bio ?? user.Bio;
            user.LocationLatitude = request.LocationLatitude ?? user.LocationLatitude;
            user.LocationLongitude = request.LocationLongitude ?? user.LocationLongitude;
        }

        // Map PlantRequest to Plant
        public static Plant MapToPlant(PlantRequest request)
        {
            if (request == null)
                return null;

            return new Plant
            {
                UserId = request.UserId,
                SpeciesName = request.SpeciesName,
                CareRequirements = request.CareRequirements,
                Description = request.Description,
                Category = request.Category,
                ImageUrl = request.ImageUrl
            };
        }

        // Map PlantUpdateRequest to existing Plant
        public static void MapToPlant(PlantUpdateRequest request, Plant plant)
        {
            if (request == null || plant == null)
                return;

            plant.SpeciesName = request.SpeciesName ?? plant.SpeciesName;
            plant.CareRequirements = request.CareRequirements ?? plant.CareRequirements;
            plant.Description = request.Description ?? plant.Description;
            plant.Category = request.Category ?? plant.Category;
            plant.ImageUrl = request.ImageUrl ?? plant.ImageUrl;
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

    }
}
