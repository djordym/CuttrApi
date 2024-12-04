using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Contracts.Outputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Managers
{
    public class ReportManager : IReportManager
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ReportManager> _logger;

        public ReportManager(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            ILogger<ReportManager> logger)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ReportResponse> CreateReportAsync(ReportRequest request, int reporterUserId)
        {
            try
            {
                // Validate that the reported user exists
                var reportedUser = await _userRepository.GetUserByIdAsync(request.ReportedUserId);
                if (reportedUser == null)
                {
                    throw new NotFoundException($"Reported user with ID {request.ReportedUserId} not found.");
                }

                // Create Report entity
                var report = new Report
                {
                    ReporterUserId = reporterUserId,
                    ReportedUserId = request.ReportedUserId,
                    Reason = request.Reason,
                    Comments = request.Comments,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                var createdReport = await _reportRepository.AddReportAsync(report);

                // Map to ReportResponse
                var response = BusinessToContractMapper.MapToReportResponse(createdReport);

                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report.");
                throw new BusinessException("Error creating report.", ex);
            }
        }
    }
}
