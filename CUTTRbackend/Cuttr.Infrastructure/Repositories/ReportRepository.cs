using Cuttr.Business.Entities;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Infrastructure.Exceptions;
using Cuttr.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly CuttrDbContext _context;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(CuttrDbContext context, ILogger<ReportRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Report> AddReportAsync(Report report)
        {
            try
            {
                var efReport = BusinessToEFMapper.MapToReportEF(report);
                efReport.ReportId = 0; // Ensure the ID is unset for new entities

                await _context.Reports.AddAsync(efReport);
                await _context.SaveChangesAsync();
                _context.Entry(efReport).State = EntityState.Detached;
                return EFToBusinessMapper.MapToReport(efReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a report.");
                throw new RepositoryException("An error occurred while adding a report.", ex);
            }
        }
    }
}
