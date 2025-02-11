using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Outputs
{
    public class ReportResponse
    {
        public int ReportId { get; set; }
        public int ReporterUserId { get; set; }
        public int ReportedUserId { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
