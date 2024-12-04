using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class UserPreferencesResponse
    {
        public int UserId { get; set; }
        public double SearchRadius { get; set; }
        public List<string> PreferredCategories { get; set; }
    }

}
