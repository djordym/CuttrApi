using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Entities
{
  
    public class UserPreferencesEF
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        public int SearchRadius { get; set; } // e.g., in kilometers

        public string PreferredCategories { get; set; } // JSON serialized list

        // Navigation property
        public virtual UserEF User { get; set; }
    }

}
