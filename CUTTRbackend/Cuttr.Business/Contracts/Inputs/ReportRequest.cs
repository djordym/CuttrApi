using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class ReportRequest
    {
        public int ReporterUserId { get; set; }
        public int ReportedUserId { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
    }

}
