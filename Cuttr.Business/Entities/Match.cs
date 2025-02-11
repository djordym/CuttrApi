using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Entities
{
    public class Match
    {
        public int MatchId { get; set; }

        public int PlantId1 { get; set; }

        public int PlantId2 { get; set; }

        public int ConnectionId { get; set; }

        public DateTime CreatedAt { get; set; }

        // References to plants and users
        public Plant Plant1 { get; set; }

        public Plant Plant2 { get; set; }

        // Messages in the match
    }
}
