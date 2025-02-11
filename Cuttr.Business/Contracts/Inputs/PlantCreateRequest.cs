using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class PlantCreateRequest
    {
        public PlantRequest PlantDetails { get; set; }
        public IFormFile Image { get; set; }
    }
}
