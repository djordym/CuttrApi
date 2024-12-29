using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(CuttrDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            try
            {
                var efMessage = BusinessToEFMapper.MapToMessageEF(message);
                efMessage.MessageId = 0; // Ensure the ID is unset for new entities

                await _context.Messages.AddAsync(efMessage);
                await _context.SaveChangesAsync();
                _context.Entry(efMessage).State = EntityState.Detached;
                return EFToBusinessMapper.MapToMessage(efMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a message.");
                throw new RepositoryException("An error occurred while adding a message.", ex);
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByMatchIdAsync(int matchId)
        {
            try
            {
                var efMessages = await _context.Messages
                    .AsNoTracking()
                    .Where(m => m.MatchId == matchId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                return efMessages.Select(EFToBusinessMapper.MapToMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving messages for match with ID {matchId}.");
                throw new RepositoryException("An error occurred while retrieving messages.", ex);
            }
        }
    }
}
