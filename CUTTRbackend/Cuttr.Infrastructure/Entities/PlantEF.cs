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

        public string? Description { get; set; }

        // Enum properties stored as strings
        [Required]
        [MaxLength(50)]
        public string PlantStage { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlantCategory { get; set; }

        [Required]
        [MaxLength(50)]
        public string WateringNeed { get; set; }

        [Required]
        [MaxLength(50)]
        public string LightRequirement { get; set; }

        [MaxLength(50)]
        public string Size { get; set; }

        [MaxLength(50)]
        public string IndoorOutdoor { get; set; }

        [MaxLength(50)]
        public string PropagationEase { get; set; }

        [MaxLength(50)]
        public string PetFriendly { get; set; }

        public string Extras { get; set; } // Assuming serialized JSON string

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsTraded { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual UserEF User { get; set; }
    }
}
