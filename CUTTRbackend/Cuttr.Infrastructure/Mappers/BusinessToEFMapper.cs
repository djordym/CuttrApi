using Cuttr.Business.Entities;
using Cuttr.Business.Enums;
using Cuttr.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Mappers
{
    public static class BusinessToEFMapper
    {
        // Map User to UserEF
        public static UserEF MapToUserEF(User user)
        {
            if (user == null)
                return null;

            return new UserEF
            {
                UserId = user.UserId,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                Plants = user.Plants?.Select(MapToPlantEFWithoutUser).ToList(),
                Preferences = MapToUserPreferencesEF(user.Preferences),
                // CreatedAt and UpdatedAt are handled by EF Core
            };
        }

        // Map Plant to PlantEF
        public static PlantEF MapToPlantEF(Plant plant)
        {
            if (plant == null)
                return null;

            return new PlantEF
            {
                PlantId = plant.PlantId,
                UserId = plant.UserId,
                SpeciesName = plant.SpeciesName,
                Description = plant.Description,
                PlantStage = plant.PlantStage.ToString(),
                PlantCategory = plant.PlantCategory.ToString(),
                WateringNeed = plant.WateringNeed.ToString(),
                LightRequirement = plant.LightRequirement.ToString(),
                Size = plant.Size.ToString(),
                IndoorOutdoor = plant.IndoorOutdoor.ToString(),
                PropagationEase = plant.PropagationEase.ToString(),
                PetFriendly = plant.PetFriendly.ToString(),
                Extras = plant.Extras != null ? SerializeExtras(plant.Extras) : null,
                ImageUrl = plant.ImageUrl,
                User = MapToUserEFWithoutPlants(plant.User),
                // CreatedAt and UpdatedAt are handled by EF Core
            };
        }

        // Helper method to map Plant without User to prevent circular reference
        private static PlantEF MapToPlantEFWithoutUser(Plant plant)
        {
            if (plant == null)
                return null;

            return new PlantEF
            {
                PlantId = plant.PlantId,
                UserId = plant.UserId,
                SpeciesName = plant.SpeciesName,
                Description = plant.Description,
                PlantStage = plant.PlantStage.ToString(),
                PlantCategory = plant.PlantCategory.ToString(),
                WateringNeed = plant.WateringNeed.ToString(),
                LightRequirement = plant.LightRequirement.ToString(),
                Size = plant.Size.ToString(),
                IndoorOutdoor = plant.IndoorOutdoor.ToString(),
                PropagationEase = plant.PropagationEase.ToString(),
                PetFriendly = plant.PetFriendly.ToString(),
                Extras = plant.Extras != null ? SerializeExtras(plant.Extras) : null,
                ImageUrl = plant.ImageUrl,
                // User is not mapped to prevent circular reference
            };
        }

        // Map User to UserEF without Plants to prevent circular reference
        private static UserEF MapToUserEFWithoutPlants(User user)
        {
            if (user == null)
                return null;

            return new UserEF
            {
                UserId = user.UserId,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                // Location is not mapped for now to keep it simple, and should also not be necessary for now, location is updated through different means
                // Plants are not mapped to prevent circular reference
                Preferences = MapToUserPreferencesEF(user.Preferences),
                // CreatedAt and UpdatedAt are handled by EF Core
            };
        }

        // Map Swipe to SwipeEF
        public static SwipeEF MapToSwipeEF(Swipe swipe)
        {
            if (swipe == null)
                return null;

            return new SwipeEF
            {
                SwipeId = swipe.SwipeId,
                SwiperPlantId = swipe.SwiperPlantId,
                SwipedPlantId = swipe.SwipedPlantId,
                IsLike = swipe.IsLike,
                SwiperPlant = MapToPlantEFWithoutUser(swipe.SwiperPlant),
                SwipedPlant = MapToPlantEFWithoutUser(swipe.SwipedPlant),
                // CreatedAt is handled by EF Core
            };
        }

        // Map Match to MatchEF
        public static MatchEF MapToMatchEF(Match match)
        {
            if (match == null)
                return null;

            return new MatchEF
            {
                MatchId = match.MatchId,
                PlantId1 = match.PlantId1,
                PlantId2 = match.PlantId2,
                UserId1 = match.UserId1,
                UserId2 = match.UserId2,
                Plant1 = MapToPlantEFWithoutUser(match.Plant1),
                Plant2 = MapToPlantEFWithoutUser(match.Plant2),
                User1 = MapToUserEFWithoutPlants(match.User1),
                User2 = MapToUserEFWithoutPlants(match.User2),
                Messages = match.Messages?.Select(MapToMessageEF).ToList(),
                CreatedAt = match.CreatedAt,
            };
        }

        // Map Message to MessageEF
        public static MessageEF MapToMessageEF(Message message)
        {
            if (message == null)
                return null;

            return new MessageEF
            {
                MessageId = message.MessageId,
                MatchId = message.MatchId,
                SenderUserId = message.SenderUserId,
                MessageText = message.MessageText,
                IsRead = message.IsRead,
                // CreatedAt in EF entity is mapped from SentAt in business entity
                CreatedAt = message.SentAt,
                SenderUser = MapToUserEFWithoutPlants(message.SenderUser),
            };
        }

        // Map Report to ReportEF
        public static ReportEF MapToReportEF(Report report)
        {
            if (report == null)
                return null;

            return new ReportEF
            {
                ReportId = report.ReportId,
                ReporterUserId = report.ReporterUserId,
                ReportedUserId = report.ReportedUserId,
                Reason = report.Reason,
                Comments = report.Comments,
                IsResolved = report.IsResolved,
                ReporterUser = MapToUserEFWithoutPlants(report.ReporterUser),
                ReportedUser = MapToUserEFWithoutPlants(report.ReportedUser),
                CreatedAt = report.CreatedAt,
            };
        }

        // Map UserPreferences to UserPreferencesEF
        public static UserPreferencesEF MapToUserPreferencesEF(UserPreferences preferences)
        {
            if (preferences == null)
                return null;

            return new UserPreferencesEF
            {
                UserId = preferences.UserId,
                SearchRadius = preferences.SearchRadius,
                PreferredCategories = SerializeCategories(preferences.PreferredCategories),
                // User is not mapped to prevent circular reference
            };
        }

        // Helper method to serialize PreferredCategories
        public static string SerializeCategories(List<string> categories)
        {
            if (categories == null || !categories.Any())
                return null;

            // Assuming JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(categories);
        }

        // Helper method to serialize List of Enums
        public static string SerializeExtras(List<Extras> extras)
        {
            if (extras == null || !extras.Any())
                return null;

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(extras);
        }
    }
}
