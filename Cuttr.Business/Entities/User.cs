using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Name { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string Bio { get; set; }
        
        public double? LocationLatitude { get; set; }

        public double? LocationLongitude { get; set; }
        public string ExpoPushToken { get; set; }

        // Business entities for related data
        public List<Plant> Plants { get; set; }

        public UserPreferences Preferences { get; set; }

        // Other properties as needed for business logic
    }
}
