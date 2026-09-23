using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportManagementService _service;

    public ReportsController(
        IReportManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "REPORT_VIEW")]
    public async Task<ActionResult<ReportListResponse>> GetReports(
        [FromQuery] ReportListRequest request)
    {
        var result = await _service.GetReportsAsync(request);

        return Ok(result);
    }
}