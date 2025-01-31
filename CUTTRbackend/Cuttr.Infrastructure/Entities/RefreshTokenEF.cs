using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Entities
{
    public class RefreshTokenEF
    {
        [Key]
        public int RefreshTokenId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(512)] // Adjust based on hashing algorithm
        public string TokenHash { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        // Navigation property
        public UserEF User { get; set; }
    }

}
