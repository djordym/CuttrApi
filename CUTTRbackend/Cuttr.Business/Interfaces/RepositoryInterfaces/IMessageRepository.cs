using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(Message message);
        Task<IEnumerable<Message>> GetMessagesByConnectionIdAsync(int matchId);
    }
}
