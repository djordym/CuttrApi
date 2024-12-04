using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class UserUpdateRequest
    {
        public string Name { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Bio { get; set; }
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }
    }
}
