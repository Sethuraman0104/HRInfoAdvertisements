using HRInfoAdvertisements.Application.DTOs.Dashboard;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly IDashboardStatisticsService _statisticsService;

    public DashboardController(
        IDashboardService service,
        IDashboardStatisticsService statisticsService)
    {
        _service = service;
        _statisticsService = statisticsService;
    }

    [HttpGet("summary")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("advertisements-by-status")]
[Authorize(Policy = "USER_VIEW")]
public async Task<ActionResult<List<AdvertisementStatusStatisticsResponse>>>
    GetAdvertisementsByStatus()
{
    var result = await _service.GetAdvertisementsByStatusAsync();
    return Ok(result);
}

[HttpGet("advertisements-by-category")]
[Authorize(Policy = "USER_VIEW")]
public async Task<ActionResult<List<AdvertisementCategoryStatisticsResponse>>>
    GetAdvertisementsByCategory()
{
    var result = await _service.GetAdvertisementsByCategoryAsync();
    return Ok(result);
}

[HttpGet("reports-by-reason")]
[Authorize(Policy = "USER_VIEW")]
public async Task<ActionResult<List<ReportReasonStatisticsResponse>>>
    GetReportsByReason()
{
    var result = await _service.GetReportsByReasonAsync();
    return Ok(result);
}

    [HttpGet("statistics")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<ActionResult<DashboardStatisticsResponse>> GetStatistics(
        [FromQuery] DashboardStatisticsRequest request)
    {
        var result = await _statisticsService.GetStatisticsAsync(request);
        return Ok(result);
    }
}