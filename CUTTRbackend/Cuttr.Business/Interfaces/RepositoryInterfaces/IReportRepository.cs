using Cuttr.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
    public interface IReportRepository
    {
        Task<Report> AddReportAsync(Report report);
        // Additional methods can be added for admin functionalities (e.g., GetReports, UpdateReport)
    }
}
