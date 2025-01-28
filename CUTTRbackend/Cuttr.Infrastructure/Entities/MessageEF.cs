using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;

namespace Cuttr.Infrastructure.Entities
{
    public class MessageEF : ICreatedAt
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConnectionId { get; set; }

        [Required]
        public int SenderUserId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public DateTime CreatedAt { get; set; } // SentAt

        public bool IsRead { get; set; }

        // Navigation properties
        [ForeignKey("MatchId")]
        public virtual ConnectionEF Connection { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual UserEF SenderUser { get; set; }
    }
}
