using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;

namespace Cuttr.Infrastructure.Entities
{
    public class SwipeEF : ICreatedAt
    {
        [Key]
        public int SwipeId { get; set; }

        [Required]
        public int SwiperPlantId { get; set; }

        [Required]
        public int SwipedPlantId { get; set; }

        [Required]
        public bool IsLike { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("SwiperPlantId")]
        public virtual PlantEF SwiperPlant { get; set; }

        [ForeignKey("SwipedPlantId")]
        public virtual PlantEF SwipedPlant { get; set; }
    }
}
