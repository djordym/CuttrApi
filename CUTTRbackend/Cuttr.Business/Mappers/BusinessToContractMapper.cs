using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;

namespace Cuttr.Business.Mappers
{
    public static class BusinessToContractMapper
    {
        public static UserResponse MapToUserResponse(User user)
        {
            if (user == null)
                return null;

            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                LocationLatitude = user.LocationLatitude,
                LocationLongitude = user.LocationLongitude
                // Exclude PasswordHash and other sensitive data
            };
        }

        // Map User and Token to UserLoginResponse
        public static UserLoginResponse MapToUserLoginResponse(User user, string token)
        {
            return new UserLoginResponse
            {
                Token = token,
                User = MapToUserResponse(user)
            };
        }

        public static PlantResponse MapToPlantResponse(Plant plant)
        {
            if (plant == null)
                return null;

            return new PlantResponse
            {
                PlantId = plant.PlantId,
                UserId = plant.UserId,
                SpeciesName = plant.SpeciesName,
                CareRequirements = plant.CareRequirements,
                Description = plant.Description,
                Category = plant.Category,
                ImageUrl = plant.ImageUrl
                // Exclude any internal fields
            };
        }

        // Map collection of Plant to collection of PlantResponse
        public static IEnumerable<PlantResponse> MapToPlantResponse(IEnumerable<Plant> plants)
        {
            return plants?.Select(MapToPlantResponse);
        }

        public static MatchResponse MapToMatchResponse(Match match)
        {
            if (match == null)
                return null;

            return new MatchResponse
            {
                MatchId = match.MatchId,
                Plant1 = MapToPlantResponse(match.Plant1),
                Plant2 = MapToPlantResponse(match.Plant2),
                User1 = MapToUserResponse(match.User1),
                User2 = MapToUserResponse(match.User2)
            };
        }
    }
}
