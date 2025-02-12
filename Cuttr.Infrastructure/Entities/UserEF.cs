using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;
using NetTopologySuite.Geometries;

namespace Cuttr.Infrastructure.Entities
{
    public class UserEF : ICreatedAt, IUpdatedAt
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public Point? Location { get; set; }

        [MaxLength(512)]
        public string? ExpoPushToken { get; set; }

        // Navigation properties
        public virtual ICollection<PlantEF> Plants { get; set; }

        public virtual UserPreferencesEF Preferences { get; set; }

        public virtual ICollection<MessageEF> SentMessages { get; set; }

        public virtual ICollection<ReportEF> ReportsMade { get; set; }

        public virtual ICollection<ReportEF> ReportsReceived { get; set; }
        public virtual ICollection<RefreshTokenEF> RefreshTokens { get; set; }

    }
}
