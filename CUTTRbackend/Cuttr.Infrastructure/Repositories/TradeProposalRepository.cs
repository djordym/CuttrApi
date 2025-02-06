using Cuttr.Business.Entities;
using Cuttr.Business.Managers;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class TradeProposalRepository : ITradeProposalRepository
    {
        private readonly CuttrDbContext _dbContext;

        public TradeProposalRepository(CuttrDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TradeProposal> GetByIdAsync(int proposalId)
        {
            var ef = await _dbContext.TradeProposals.AsNoTracking()
                .Include(tp => tp.Connection)
                .Include(tp => tp.TradeProposalPlants)
                    .ThenInclude(tpp => tpp.Plant)
                .FirstOrDefaultAsync(tp => tp.TradeProposalId == proposalId);

            if (ef == null)
                return null;

            return EFToBusinessMapper.MapToTradeProposal(ef);
        }

        public async Task<IEnumerable<TradeProposal>> GetByConnectionIdAsync(int connectionId)
        {
            var efList = await _dbContext.TradeProposals.AsNoTracking()
                .Include(tp => tp.Connection)
                .Include(tp => tp.TradeProposalPlants)
                    .ThenInclude(tpp => tpp.Plant)
                .Where(tp => tp.ConnectionId == connectionId)
                .OrderBy(tp => tp.CreatedAt)
                .ToListAsync();

            return efList.Select(EFToBusinessMapper.MapToTradeProposal);
        }

        public async Task<TradeProposal> CreateAsync(TradeProposal proposal)
        {
            // Map the business model to the new EF entity structure.
            var ef = BusinessToEFMapper.MapToTradeProposalEF(proposal);

            _dbContext.TradeProposals.Add(ef);
            await _dbContext.SaveChangesAsync();

            return EFToBusinessMapper.MapToTradeProposal(ef);
        }

        public async Task<TradeProposal> UpdateAsync(TradeProposal proposal)
        {
            var ef = BusinessToEFMapper.MapToTradeProposalEF(proposal);

            _dbContext.TradeProposals.Update(ef);
            await _dbContext.SaveChangesAsync();

            return EFToBusinessMapper.MapToTradeProposal(ef);
        }



    }
}
