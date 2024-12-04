using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
    public interface IReportManager
    {
        Task<ReportResponse> CreateReportAsync(ReportRequest request, int reporterUserId);
        // Additional methods can be added for admin functionalities (e.g., GetReports, ResolveReport)
    }
}
