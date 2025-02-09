using Cuttr.Business.Entities;
using Cuttr.Business.Enums;
using Cuttr.Infrastructure.Entities;
using NetTopologySuite.Geometries;
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

            NetTopologySuite.Geometries.Point location = null;
            if (user.LocationLongitude.HasValue && user.LocationLatitude.HasValue)
            {
                location = new NetTopologySuite.Geometries.Point(
                    user.LocationLongitude.Value,
                    user.LocationLatitude.Value
                )
                {
                    SRID = 4326
                };
            }

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
                Location = location,
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
                IsTraded = plant.IsTraded,
                // CreatedAt and UpdatedAt are handled by EF Core
            };
        }

        // Helper method to map Plant without User to prevent circular reference
        public static PlantEF MapToPlantEFWithoutUser(Plant plant)
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
                IsTraded = plant.IsTraded,
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
                ConnectionId = match.ConnectionId,
                Plant1 = MapToPlantEFWithoutUser(match.Plant1),
                Plant2 = MapToPlantEFWithoutUser(match.Plant2),
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
                ConnectionId = message.ConnectionId,
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

        public static ConnectionEF MapToConnectionEF(Connection match)
        {
            if (match == null)
                return null;

            return new ConnectionEF
            {
                ConnectionId = match.ConnectionId,
                UserId1 = match.UserId1,
                UserId2 = match.UserId2,
                User1 = MapToUserEFWithoutPlants(match.User1),
                User2 = MapToUserEFWithoutPlants(match.User2),
                Messages = match.Messages?.Select(MapToMessageEF).ToList(),
                CreatedAt = match.CreatedAt,
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
                PreferedPlantStage = SerializePreferedPlantStages(preferences.PreferedPlantStage),
                PreferedPlantCategory = SerializePreferedPlantCategories(preferences.PreferedPlantCategory),
                PreferedWateringNeed = SerializePreferedWateringNeeds(preferences.PreferedWateringNeed),
                PreferedLightRequirement = SerializePreferedLightRequirements(preferences.PreferedLightRequirement),
                PreferedSize = SerializePreferedSizes(preferences.PreferedSize),
                PreferedIndoorOutdoor = SerializePreferedIndoorOutdoors(preferences.PreferedIndoorOutdoor),
                PreferedPropagationEase = SerializePreferedPropagationEases(preferences.PreferedPropagationEase),
                PreferedPetFriendly = SerializePreferedPetFriendlies(preferences.PreferedPetFriendly),
                PreferedExtras = SerializeExtras(preferences.PreferedExtras),       
                // User is not mapped to prevent circular reference
            };
        }

        public static TradeProposalEF MapToTradeProposalEF(TradeProposal proposal)
        {
            if (proposal == null)
                return null;

            var ef = new TradeProposalEF
            {
                TradeProposalId = proposal.TradeProposalId,
                ConnectionId = proposal.ConnectionId,
                ProposalOwnerUserId = proposal.ProposalOwnerUserId,
                TradeProposalStatus = proposal.TradeProposalStatus.ToString(),
                CreatedAt = proposal.CreatedAt,
                AcceptedAt = proposal.AcceptedAt,
                DeclinedAt = proposal.DeclinedAt,
                CompletedAt = proposal.CompletedAt,
                OwnerCompletionConfirmed = proposal.OwnerCompletionConfirmed,
                ResponderCompletionConfirmed = proposal.ResponderCompletionConfirmed
            };

            // Map the plants proposed by User1.
            if (proposal.PlantIdsProposedByUser1 != null)
            {
                foreach (var plantId in proposal.PlantIdsProposedByUser1)
                {
                    ef.TradeProposalPlants.Add(new TradeProposalPlantEF
                    {
                        TradeProposalId = proposal.TradeProposalId,
                        PlantId = plantId,
                        IsProposedByUser1 = true
                    });
                }
            }

            // Map the plants proposed by User2.
            if (proposal.PlantIdsProposedByUser2 != null)
            {
                foreach (var plantId in proposal.PlantIdsProposedByUser2)
                {
                    ef.TradeProposalPlants.Add(new TradeProposalPlantEF
                    {
                        TradeProposalId = proposal.TradeProposalId,
                        PlantId = plantId,
                        IsProposedByUser1 = false
                    });
                }
            }

            return ef;
        }

        public static string SerializePreferedPlantStages(List<PlantStage> plantStages)
        {
            if (plantStages == null || !plantStages.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(plantStages);
        }
        public static string SerializePreferedPlantCategories(List<PlantCategory> plantCategories)
        {
            if (plantCategories == null || !plantCategories.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(plantCategories);
        }
        public static string SerializePreferedWateringNeeds(List<WateringNeed> wateringNeeds)
        {
            if (wateringNeeds == null || !wateringNeeds.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(wateringNeeds);
        }
        public static string SerializePreferedLightRequirements(List<LightRequirement> lightRequirements)
        {
            if (lightRequirements == null || !lightRequirements.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(lightRequirements);
        }
        public static string SerializePreferedSizes(List<Size> sizes)
        {
            if (sizes == null || !sizes.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(sizes);
        }
        public static string SerializePreferedIndoorOutdoors(List<IndoorOutdoor> indoorOutdoors)
        {
            if (indoorOutdoors == null || !indoorOutdoors.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(indoorOutdoors);
        }
        public static string SerializePreferedPropagationEases(List<PropagationEase> propagationEases)
        {
            if (propagationEases == null || !propagationEases.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(propagationEases);
        }
        public static string SerializePreferedPetFriendlies(List<PetFriendly> petFriendlies)
        {
            if (petFriendlies == null || !petFriendlies.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(petFriendlies);
        }
        public static string SerializeExtras(List<Extras> extras)
        {
            if (extras == null || !extras.Any())
                return "";
            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(extras);
        }

        public static string SerializePlantIds(List<int> plantIds)
        {
            if (plantIds == null || !plantIds.Any())
                return "";

            // JSON serialization
            return System.Text.Json.JsonSerializer.Serialize(plantIds);
        }
    }
}
