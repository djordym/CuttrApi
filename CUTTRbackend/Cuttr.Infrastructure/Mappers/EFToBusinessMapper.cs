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
                LocationLatitude = efUser.Location?.Y,
                LocationLongitude = efUser.Location?.X,
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
                Description = efPlant.Description,

                // Required enum property parsed directly
                PlantStage = Enum.Parse<PlantStage>(efPlant.PlantStage),

                // Nullable enum properties: check for null or empty and assign accordingly
                PlantCategory = !string.IsNullOrWhiteSpace(efPlant.PlantCategory)
                        ? Enum.Parse<PlantCategory>(efPlant.PlantCategory)
                        : null,

                WateringNeed = !string.IsNullOrWhiteSpace(efPlant.WateringNeed)
                        ? Enum.Parse<WateringNeed>(efPlant.WateringNeed)
                        : null,

                LightRequirement = !string.IsNullOrWhiteSpace(efPlant.LightRequirement)
                        ? Enum.Parse<LightRequirement>(efPlant.LightRequirement)
                        : null,

                Size = !string.IsNullOrWhiteSpace(efPlant.Size)
                        ? Enum.Parse<Size>(efPlant.Size)
                        : null,

                IndoorOutdoor = !string.IsNullOrWhiteSpace(efPlant.IndoorOutdoor)
                        ? Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor)
                        : null,

                PropagationEase = !string.IsNullOrWhiteSpace(efPlant.PropagationEase)
                        ? Enum.Parse<PropagationEase>(efPlant.PropagationEase)
                        : null,

                PetFriendly = !string.IsNullOrWhiteSpace(efPlant.PetFriendly)
                        ? Enum.Parse<PetFriendly>(efPlant.PetFriendly)
                        : null,

                Extras = efPlant.Extras != null
                ? DeserializeExtras(efPlant.Extras)
                : null,

                ImageUrl = efPlant.ImageUrl,
                User = MapToUserWithoutPlants(efPlant.User),
                IsTraded = efPlant.IsTraded,
                // Exclude CreatedAt and UpdatedAt
            };


        }

        // Helper method to map PlantEF without User to prevent circular reference
        public static Plant MapToPlantWithoutUser(PlantEF efPlant)
        {
            if (efPlant == null)
                return null;

            return new Plant
            {
                PlantId = efPlant.PlantId,
                UserId = efPlant.UserId,
                SpeciesName = efPlant.SpeciesName,
                Description = efPlant.Description,

                // Required enum property parsed directly
                PlantStage = Enum.Parse<PlantStage>(efPlant.PlantStage),

                // Nullable enum properties: check for null or empty and assign accordingly
                PlantCategory = !string.IsNullOrWhiteSpace(efPlant.PlantCategory)
                        ? Enum.Parse<PlantCategory>(efPlant.PlantCategory)
                        : null,

                WateringNeed = !string.IsNullOrWhiteSpace(efPlant.WateringNeed)
                        ? Enum.Parse<WateringNeed>(efPlant.WateringNeed)
                        : null,

                LightRequirement = !string.IsNullOrWhiteSpace(efPlant.LightRequirement)
                        ? Enum.Parse<LightRequirement>(efPlant.LightRequirement)
                        : null,

                Size = !string.IsNullOrWhiteSpace(efPlant.Size)
                        ? Enum.Parse<Size>(efPlant.Size)
                        : null,

                IndoorOutdoor = !string.IsNullOrWhiteSpace(efPlant.IndoorOutdoor)
                        ? Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor)
                        : null,

                PropagationEase = !string.IsNullOrWhiteSpace(efPlant.PropagationEase)
                        ? Enum.Parse<PropagationEase>(efPlant.PropagationEase)
                        : null,

                PetFriendly = !string.IsNullOrWhiteSpace(efPlant.PetFriendly)
                        ? Enum.Parse<PetFriendly>(efPlant.PetFriendly)
                        : null,

                Extras = efPlant.Extras != null
                ? DeserializeExtras(efPlant.Extras)
                : null,

                ImageUrl = efPlant.ImageUrl,
                IsTraded = efPlant.IsTraded,
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
                Name = efUser.Name,
                ProfilePictureUrl = efUser.ProfilePictureUrl,
                Bio = efUser.Bio,
                // Location is updated seperately and shouldn't be necessary to be mapped
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
                ConnectionId = efMatch.ConnectionId,
                Plant1 = MapToPlantWithoutUser(efMatch.Plant1),
                Plant2 = MapToPlantWithoutUser(efMatch.Plant2),
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
                ConnectionId = efMessage.ConnectionId,
                SenderUserId = efMessage.SenderUserId,
                MessageText = efMessage.MessageText,
                IsRead = efMessage.IsRead,
                SentAt = efMessage.CreatedAt, // Mapping CreatedAt to SentAt
                // Optionally include SenderUser without references to prevent circular references
                SenderUser = MapToUserWithoutPlants(efMessage.SenderUser),
            };
        }
        public static Connection MapToConnection(ConnectionEF efConnection)
        {
            if (efConnection == null)
                return null;

            return new Connection
            {
                ConnectionId = efConnection.ConnectionId,
                UserId1 = efConnection.UserId1,
                UserId2 = efConnection.UserId2,
                User1 = MapToUserWithoutPlants(efConnection.User1),
                User2 = MapToUserWithoutPlants(efConnection.User2),
                Messages = efConnection.Messages?.Select(MapToMessage).ToList(),
                CreatedAt = efConnection.CreatedAt,
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
                PreferedPlantStage = DeserializePlantStage(efPreferences.PreferedPlantStage),
                PreferedPlantCategory = DeserializePlantCategory(efPreferences.PreferedPlantCategory),
                PreferedWateringNeed = DeserializeWateringNeed(efPreferences.PreferedWateringNeed),
                PreferedLightRequirement = DeserializeLightRequirement(efPreferences.PreferedLightRequirement),
                PreferedSize = DeserializeSize(efPreferences.PreferedSize),
                PreferedIndoorOutdoor = DeserializeIndoorOutdoor(efPreferences.PreferedIndoorOutdoor),
                PreferedPropagationEase = DeserializePropagationEase(efPreferences.PreferedPropagationEase),
                PreferedPetFriendly = DeserializePetFriendly(efPreferences.PreferedPetFriendly),
                PreferedExtras = DeserializeExtras(efPreferences.PreferedExtras),
            };
        }

        public static TradeProposal MapToTradeProposal(TradeProposalEF ef)
        {
            if (ef == null)
                return null;

            var tradeProposal = new TradeProposal
            {
                TradeProposalId = ef.TradeProposalId,
                ConnectionId = ef.ConnectionId,
                TradeProposalStatus = !string.IsNullOrWhiteSpace(ef.TradeProposalStatus)
                        ? Enum.Parse<TradeProposalStatus>(ef.TradeProposalStatus)
                        : TradeProposalStatus.Pending,
                CreatedAt = ef.CreatedAt,
                AcceptedAt = ef.AcceptedAt,
                DeclinedAt = ef.DeclinedAt,
                CompletedAt = ef.CompletedAt,
                Connection = MapToConnection(ef.Connection),
                PlantIdsProposedByUser1 = ef.TradeProposalPlants
                    .Where(tpp => tpp.IsProposedByUser1)
                    .Select(tpp => tpp.PlantId)
                    .ToList(),
                PlantIdsProposedByUser2 = ef.TradeProposalPlants
                    .Where(tpp => !tpp.IsProposedByUser1)
                    .Select(tpp => tpp.PlantId)
                    .ToList(),
                PlantsProposedByUser1 = ef.TradeProposalPlants
                    .Where(tpp => tpp.IsProposedByUser1 && tpp.Plant != null)
                    .Select(tpp => MapToPlant(tpp.Plant))
                    .ToList(),
                PlantsProposedByUser2 = ef.TradeProposalPlants
                    .Where(tpp => !tpp.IsProposedByUser1 && tpp.Plant != null)
                    .Select(tpp => MapToPlant(tpp.Plant))
                    .ToList(),
                ProposalOwnerUserId = ef.ProposalOwnerUserId,
                OwnerCompletionConfirmed = ef.OwnerCompletionConfirmed,
                ResponderCompletionConfirmed = ef.ResponderCompletionConfirmed
            };


            return tradeProposal;

        }

        // Helper method to deserialize Plantstage
        private static List<PlantStage> DeserializePlantStage(string plantstages)
        {
            if (string.IsNullOrEmpty(plantstages))
                return new List<PlantStage>();
            return System.Text.Json.JsonSerializer.Deserialize<List<PlantStage>>(plantstages);
        }

        // Helper Method to deserialize PlantCategory

        private static List<PlantCategory> DeserializePlantCategory(string plantcategories)
        {
            if (string.IsNullOrEmpty(plantcategories))
                return new List<PlantCategory>();
            return System.Text.Json.JsonSerializer.Deserialize<List<PlantCategory>>(plantcategories);
        }

        // Helper method to deserialize WateringNeed

        private static List<WateringNeed> DeserializeWateringNeed(string wateringneeds)
        {
            if (string.IsNullOrEmpty(wateringneeds))
                return new List<WateringNeed>();
            return System.Text.Json.JsonSerializer.Deserialize<List<WateringNeed>>(wateringneeds);
        }

        // Helper method to deserialize LightRequirement

        private static List<LightRequirement> DeserializeLightRequirement(string lightrequirements)
        {
            if (string.IsNullOrEmpty(lightrequirements))
                return new List<LightRequirement>();
            return System.Text.Json.JsonSerializer.Deserialize<List<LightRequirement>>(lightrequirements);
        }

        // Helper method to deserialize Size

        private static List<Size> DeserializeSize(string sizes)
        {
            if (string.IsNullOrEmpty(sizes))
                return new List<Size>();
            return System.Text.Json.JsonSerializer.Deserialize<List<Size>>(sizes);
        }

        // Helper method to deserialize IndoorOutdoor

        private static List<IndoorOutdoor> DeserializeIndoorOutdoor(string indooroutdoors)
        {
            if (string.IsNullOrEmpty(indooroutdoors))
                return new List<IndoorOutdoor>();
            return System.Text.Json.JsonSerializer.Deserialize<List<IndoorOutdoor>>(indooroutdoors);
        }

        // Helper method to deserialize PropagationEase

        private static List<PropagationEase> DeserializePropagationEase(string propagationeases)


        {
            if (string.IsNullOrEmpty(propagationeases))
                return new List<PropagationEase>();
            return System.Text.Json.JsonSerializer.Deserialize<List<PropagationEase>>(propagationeases);
        }

        // Helper method to deserialize PetFriendly

        private static List<PetFriendly> DeserializePetFriendly(string petfriendlies)
        {
            if (string.IsNullOrEmpty(petfriendlies))
                return new List<PetFriendly>();
            return System.Text.Json.JsonSerializer.Deserialize<List<PetFriendly>>(petfriendlies);
        }

        // Helper method to deserialize Extras

        private static List<Extras> DeserializeExtras(string serializedExtras)
        {
            if (string.IsNullOrEmpty(serializedExtras))
                return new List<Extras>();

            // JSON deserialization
            return System.Text.Json.JsonSerializer.Deserialize<List<Extras>>(serializedExtras);
        }
    }
}
