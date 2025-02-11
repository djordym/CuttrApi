using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class Message
    {
        public int MessageId { get; set; }

        public int ConnectionId { get; set; }

        public int SenderUserId { get; set; }

        public string MessageText { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        // References
        public Connection Connection { get; set; }

        public User SenderUser { get; set; }
    }
}
