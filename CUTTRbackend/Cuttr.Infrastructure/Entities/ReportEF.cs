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
    public class ReportEF : ICreatedAt
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int ReporterUserId { get; set; }

        [Required]
        public int ReportedUserId { get; set; }

        [Required]
        public string Reason { get; set; }

        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } // ReportedAt

        public bool IsResolved { get; set; }

        // Navigation properties
        [ForeignKey("ReporterUserId")]
        public virtual UserEF ReporterUser { get; set; }

        [ForeignKey("ReportedUserId")]
        public virtual UserEF ReportedUser { get; set; }
    }
}
