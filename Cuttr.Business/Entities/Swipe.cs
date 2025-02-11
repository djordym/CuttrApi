using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class Swipe
    {
        public int SwipeId { get; set; }

        public int SwiperPlantId { get; set; }

        public int SwipedPlantId { get; set; }

        public bool IsLike { get; set; }

        // References to plants
        public Plant SwiperPlant { get; set; }

        public Plant SwipedPlant { get; set; }
    }
}
