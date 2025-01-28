using Cuttr.Business.Entities;

namespace Cuttr.Business.Managers
{
    public interface ITradeProposalRepository
    {
        Task<TradeProposal> GetByIdAsync(int proposalId);
        Task<IEnumerable<TradeProposal>> GetByConnectionIdAsync(int connectionId);

        Task<TradeProposal> CreateAsync(TradeProposal proposal);
        Task<TradeProposal> UpdateAsync(TradeProposal proposal);
        // Possibly a DeleteAsync if needed
    }
}