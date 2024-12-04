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
    public class MatchEF : ICreatedAt
    {
        [Key]
        public int MatchId { get; set; }

        [Required]
        public int PlantId1 { get; set; }

        [Required]
        public int PlantId2 { get; set; }

        [Required]
        public int UserId1 { get; set; } // Owner of PlantId1

        [Required]
        public int UserId2 { get; set; } // Owner of PlantId2

        public DateTime CreatedAt { get; set; } // MatchedAt

        // Navigation properties
        [ForeignKey("PlantId1")]
        public virtual PlantEF Plant1 { get; set; }

        [ForeignKey("PlantId2")]
        public virtual PlantEF Plant2 { get; set; }

        [ForeignKey("UserId1")]
        public virtual UserEF User1 { get; set; }

        [ForeignKey("UserId2")]
        public virtual UserEF User2 { get; set; }

        public virtual ICollection<MessageEF> Messages { get; set; }
    }
}
