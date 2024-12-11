using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class UserPreferences
    {
        public int UserId { get; set; }

        public int SearchRadius { get; set; }

        public List<string> PreferredCategories { get; set; } // Could be a list in the business layer

        // Reference to User
        public User User { get; set; }
    }
}
