using Azure.Core;
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

        public static PlantResponse MapToPlantResponse(Plant plant)
        {
            if (plant == null)
                return null;

            return new PlantResponse
            {
                PlantId = plant.PlantId,
                UserId = plant.UserId,
                SpeciesName = plant.SpeciesName,
                Description = plant.Description,
                ImageUrl = plant.ImageUrl,
                PlantStage = plant.PlantStage,
                PlantCategory = plant.PlantCategory,
                WateringNeed = plant.WateringNeed,
                LightRequirement = plant.LightRequirement,
                Size = plant.Size,
                IndoorOutdoor = plant.IndoorOutdoor,
                PropagationEase = plant.PropagationEase,
                PetFriendly = plant.PetFriendly,
                Extras = plant.Extras
                // Exclude any internal fields
            };
        }

        // Map collection of Plant to collection of PlantResponse
        public static IEnumerable<PlantResponse> MapToPlantResponse(IEnumerable<Plant> plants)
        {
            return plants?.Select(MapToPlantResponse);
        }

        

        public static MessageResponse MapToMessageResponse(Message message)
        {
            if (message == null)
                return null;

            return new MessageResponse
            {
                MessageId = message.MessageId,
                ConnectionId = message.ConnectionId,
                SenderUserId = message.SenderUserId,
                MessageText = message.MessageText,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };
        }

        // Map collection of Message to collection of MessageResponse
        public static IEnumerable<MessageResponse> MapToMessageResponse(IEnumerable<Message> messages)
        {
            return messages?.Select(MapToMessageResponse);
        }

        public static ReportResponse MapToReportResponse(Report report)
        {
            if (report == null)
                return null;

            return new ReportResponse
            {
                ReportId = report.ReportId,
                ReporterUserId = report.ReporterUserId,
                ReportedUserId = report.ReportedUserId,
                Reason = report.Reason,
                Comments = report.Comments,
                CreatedAt = report.CreatedAt,
                IsResolved = report.IsResolved
            };
        }

        public static UserPreferencesResponse MapToUserPreferencesResponse(UserPreferences preferences)
        {
            if (preferences == null)
                return null;

            return new UserPreferencesResponse
            {
                UserId = preferences.UserId,
                SearchRadius = preferences.SearchRadius,
                PreferedPlantStage = preferences.PreferedPlantStage,
                PreferedPlantCategory = preferences.PreferedPlantCategory,
                PreferedWateringNeed = preferences.PreferedWateringNeed,
                PreferedLightRequirement = preferences.PreferedLightRequirement,
                PreferedSize = preferences.PreferedSize,
                PreferedIndoorOutdoor = preferences.PreferedIndoorOutdoor,
                PreferedPropagationEase = preferences.PreferedPropagationEase,
                PreferedPetFriendly = preferences.PreferedPetFriendly,
                PreferedExtras = preferences.PreferedExtras
            };
        }

        public static TradeProposalResponse MapToTradeProposalResponse(TradeProposal tp)
        {
            if (tp == null)
                return null;
            return new TradeProposalResponse
            {
                TradeProposalId = tp.TradeProposalId,
                ConnectionId = tp.ConnectionId,
                PlantsProposedByUser1 = (tp.PlantsProposedByUser1).Select(MapToPlantResponse).ToList(),
                PlantsProposedByUser2 = (tp.PlantsProposedByUser2).Select(MapToPlantResponse).ToList(),
                TradeProposalStatus = tp.TradeProposalStatus,
                CreatedAt = tp.CreatedAt,
                AcceptedAt = tp.AcceptedAt,
                DeclinedAt = tp.DeclinedAt,
                CompletedAt = tp.CompletedAt,
                Connection = MapToConnectionResponse(tp.Connection),
                ProposalOwnerUserId = tp.ProposalOwnerUserId,
                OwnerCompletionConfirmed = tp.OwnerCompletionConfirmed,
                ResponderCompletionConfirmed = tp.ResponderCompletionConfirmed
            };
        }

        public static ConnectionResponse MapToConnectionResponse(Connection match)
        {
            if (match == null)
                return null;

            return new ConnectionResponse
            {
                ConnectionId = match.ConnectionId,
                User1 = MapToUserResponse(match.User1),
                User2 = MapToUserResponse(match.User2)
            };
        }

        public static IEnumerable<ConnectionResponse> MapToConnectionResponse(IEnumerable<Connection> matches)
        {
            return matches?.Select(MapToConnectionResponse);
        }

        public static MatchResponse MapToMatchResponse(Match match)
        {
            if (match == null)
                return null;

            return new MatchResponse
            {
                MatchId = match.MatchId,
                ConnectionId = match.ConnectionId,
                Plant1 = MapToPlantResponse(match.Plant1),
                Plant2 = MapToPlantResponse(match.Plant2),
            };
        }

        public static IEnumerable<MatchResponse> MapToMatchResponse(IEnumerable<Match> matches)
        {
            return matches?.Select(MapToMatchResponse);
        }
    }
}
