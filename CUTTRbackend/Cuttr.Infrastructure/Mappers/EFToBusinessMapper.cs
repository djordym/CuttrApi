using Cuttr.Business.Entities;
using Cuttr.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Mappers
{
    public static class EFToBusinessMapper
    {
        // Map UserEF to User
        public static User MapToUser(UserEF efUser)
        {
            if (efUser == null)
                return null;

            return new User
            {
                UserId = efUser.UserId,
                Email = efUser.Email,
                PasswordHash = efUser.PasswordHash,
                Name = efUser.Name,
                ProfilePictureUrl = efUser.ProfilePictureUrl,
                Bio = efUser.Bio,
                LocationLatitude = efUser.LocationLatitude,
                LocationLongitude = efUser.LocationLongitude,
                Plants = efUser.Plants?.Select(MapToPlantWithoutUser).ToList(),
                Preferences = MapToUserPreferences(efUser.Preferences),
                // Exclude CreatedAt and UpdatedAt
            };
        }

        // Map PlantEF to Plant
        public static Plant MapToPlant(PlantEF efPlant)
        {
            if (efPlant == null)
                return null;

            return new Plant
            {
                PlantId = efPlant.PlantId,
                UserId = efPlant.UserId,
                SpeciesName = efPlant.SpeciesName,
                CareRequirements = efPlant.CareRequirements,
                Description = efPlant.Description,
                Category = efPlant.Category,
                ImageUrl = efPlant.ImageUrl,
                User = MapToUserWithoutPlants(efPlant.User),
                // Exclude CreatedAt and UpdatedAt
            };
        }

        // Helper method to map PlantEF without User to prevent circular reference
        private static Plant MapToPlantWithoutUser(PlantEF efPlant)
        {
            if (efPlant == null)
                return null;

            return new Plant
            {
                PlantId = efPlant.PlantId,
                UserId = efPlant.UserId,
                SpeciesName = efPlant.SpeciesName,
                CareRequirements = efPlant.CareRequirements,
                Description = efPlant.Description,
                Category = efPlant.Category,
                ImageUrl = efPlant.ImageUrl,
                // User is not mapped to prevent circular reference
            };
        }

        // Map UserEF to User without Plants to prevent circular reference
        private static User MapToUserWithoutPlants(UserEF efUser)
        {
            if (efUser == null)
                return null;

            return new User
            {
                UserId = efUser.UserId,
                Email = efUser.Email,
                PasswordHash = efUser.PasswordHash,
                Name = efUser.Name,
                ProfilePictureUrl = efUser.ProfilePictureUrl,
                Bio = efUser.Bio,
                LocationLatitude = efUser.LocationLatitude,
                LocationLongitude = efUser.LocationLongitude,
                // Plants are not mapped to prevent circular reference
                Preferences = MapToUserPreferences(efUser.Preferences),
                // Exclude CreatedAt and UpdatedAt
            };
        }

        // Map SwipeEF to Swipe
        public static Swipe MapToSwipe(SwipeEF efSwipe)
        {
            if (efSwipe == null)
                return null;

            return new Swipe
            {
                SwipeId = efSwipe.SwipeId,
                SwiperPlantId = efSwipe.SwiperPlantId,
                SwipedPlantId = efSwipe.SwipedPlantId,
                IsLike = efSwipe.IsLike,
                SwiperPlant = MapToPlantWithoutUser(efSwipe.SwiperPlant),
                SwipedPlant = MapToPlantWithoutUser(efSwipe.SwipedPlant),
                // Exclude CreatedAt
            };
        }

        // Map MatchEF to Match
        public static Match MapToMatch(MatchEF efMatch)
        {
            if (efMatch == null)
                return null;

            return new Match
            {
                MatchId = efMatch.MatchId,
                PlantId1 = efMatch.PlantId1,
                PlantId2 = efMatch.PlantId2,
                UserId1 = efMatch.UserId1,
                UserId2 = efMatch.UserId2,
                Plant1 = MapToPlantWithoutUser(efMatch.Plant1),
                Plant2 = MapToPlantWithoutUser(efMatch.Plant2),
                User1 = MapToUserWithoutPlants(efMatch.User1),
                User2 = MapToUserWithoutPlants(efMatch.User2),
                Messages = efMatch.Messages?.Select(MapToMessage).ToList(),
                CreatedAt = efMatch.CreatedAt,
            };
        }

        // Map MessageEF to Message
        public static Message MapToMessage(MessageEF efMessage)
        {
            if (efMessage == null)
                return null;

            return new Message
            {
                MessageId = efMessage.MessageId,
                MatchId = efMessage.MatchId,
                SenderUserId = efMessage.SenderUserId,
                MessageText = efMessage.MessageText,
                IsRead = efMessage.IsRead,
                SentAt = efMessage.CreatedAt, // Mapping CreatedAt to SentAt
                // Optionally include SenderUser without references to prevent circular references
                SenderUser = MapToUserWithoutPlants(efMessage.SenderUser),
            };
        }

        // Map ReportEF to Report
        public static Report MapToReport(ReportEF efReport)
        {
            if (efReport == null)
                return null;

            return new Report
            {
                ReportId = efReport.ReportId,
                ReporterUserId = efReport.ReporterUserId,
                ReportedUserId = efReport.ReportedUserId,
                Reason = efReport.Reason,
                Comments = efReport.Comments,
                IsResolved = efReport.IsResolved,
                ReporterUser = MapToUserWithoutPlants(efReport.ReporterUser),
                ReportedUser = MapToUserWithoutPlants(efReport.ReportedUser),
                CreatedAt = efReport.CreatedAt,
            };
        }

        // Map UserPreferencesEF to UserPreferences
        public static UserPreferences MapToUserPreferences(UserPreferencesEF efPreferences)
        {
            if (efPreferences == null)
                return null;

            return new UserPreferences
            {
                UserId = efPreferences.UserId,
                SearchRadius = efPreferences.SearchRadius,
                PreferredCategories = DeserializeCategories(efPreferences.PreferredCategories),
                // User is not mapped to prevent circular reference
            };
        }

        // Helper method to deserialize PreferredCategories
        private static List<string> DeserializeCategories(string serializedCategories)
        {
            if (string.IsNullOrEmpty(serializedCategories))
                return new List<string>();

            // Assuming JSON serialization
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(serializedCategories);
        }
    }
}
