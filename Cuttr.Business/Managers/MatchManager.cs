using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class MatchManager : IMatchManager
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<MatchManager> _logger;

        public MatchManager(IMatchRepository matchRepository, ILogger<MatchManager> logger)
        {
            _matchRepository = matchRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<MatchResponse>> GetMatchesByConnectionIdAsync(int connectionId)
        {
            try
            {
                var matches = await _matchRepository.GetMatchesByConnectionIdAsync(connectionId);
                return BusinessToContractMapper.MapToMatchResponse(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving matches for user with ID {connectionId}.");
                throw new BusinessException("Error retrieving matches.", ex);
            }
        }

        public async Task<MatchResponse> GetMatchByIdAsync(int matchId)
        {
            try
            {
                var match = await _matchRepository.GetMatchByIdAsync(matchId);
                if (match == null)
                {
                    throw new NotFoundException($"Match with ID {matchId} not found.");
                }

                return BusinessToContractMapper.MapToMatchResponse(match);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving match with ID {matchId}.");
                throw new BusinessException("Error retrieving match.", ex);
            }
        }
    }
}
