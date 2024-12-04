using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IMessageManager
    {
        Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId);
        Task<IEnumerable<MessageResponse>> GetMessagesByMatchIdAsync(int matchId, int userId);
    }
}
