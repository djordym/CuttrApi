using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class SwipeRequest
    {
        public int SwiperPlantId { get; set; }
        public int SwipedPlantId { get; set; }
        public bool IsLike { get; set; }
    }
}
