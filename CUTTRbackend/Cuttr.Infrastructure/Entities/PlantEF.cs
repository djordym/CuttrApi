using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;

namespace Cuttr.Infrastructure.Entities
{
    public class PlantEF : ICreatedAt, IUpdatedAt
    {
        [Key]
        public int PlantId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SpeciesName { get; set; }

        public string CareRequirements { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual UserEF User { get; set; }
    }
}
