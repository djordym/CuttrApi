using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int MatchId { get; set; }
        public int SenderPlantId { get; set; }
        public int ReceiverPlantId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
