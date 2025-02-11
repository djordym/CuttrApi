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
        public int ConnectionId { get; set; }
        
        public DateTime CreatedAt { get; set; } // MatchedAt

        // Navigation properties
        [ForeignKey("PlantId1")]
        public virtual PlantEF Plant1 { get; set; }

        [ForeignKey("PlantId2")]
        public virtual PlantEF Plant2 { get; set; }

        [ForeignKey("ConnectionId")]
        public virtual ConnectionEF Connection { get; set; }

    }
}
