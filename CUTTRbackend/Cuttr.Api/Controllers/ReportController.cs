using Cuttr.Business.Contracts.Inputs;
using Cuttr.Business.Entities;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuttr.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportManager _reportManager;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportManager reportManager, ILogger<ReportController> logger)
        {
            _reportManager = reportManager;
            _logger = logger;
        }

        // POST: api/reports
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] ReportRequest request)
        {
            try
            {
                int reporterUserId = GetAuthenticatedUserId();

                var reportResponse = await _reportManager.CreateReportAsync(request, reporterUserId);
                return Ok(reportResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Reported user not found.");
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Error creating report.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating report.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        private int GetAuthenticatedUserId()
        {
            // Extract user ID from JWT token claims
            return int.Parse(User.FindFirst("sub")?.Value);
        }
    }
}
