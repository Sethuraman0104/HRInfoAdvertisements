using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportReviewController : ControllerBase
{
    private readonly IReportReviewService _service;

    public ReportReviewController(
        IReportReviewService service)
    {
        _service = service;
    }

    [HttpPut("{reportId:long}/review")]
    [Authorize(Policy = "REPORT_REVIEW")]
    public async Task<IActionResult> ReviewReport(
        long reportId,
        [FromBody] ReportReviewRequest request)
    {
        if (reportId <= 0)
        {
            return BadRequest(new
            {
                message = "ReportID must be greater than zero."
            });
        }

        if (request == null ||
            string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new
            {
                message = "Status is required."
            });
        }

        var allowedStatuses = new[]
        {
            "Reviewed",
            "Resolved",
            "Rejected"
        };

        if (!allowedStatuses.Contains(
                request.Status.Trim(),
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "Invalid status. Allowed values are Reviewed, Resolved, or Rejected."
            });
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(userIdClaim, out var reviewedByUserId))
        {
            return Unauthorized(new
            {
                message = "Unable to identify the current user."
            });
        }

        var result = await _service.ReviewReportAsync(
            reportId,
            request,
            reviewedByUserId);

        if (!result)
        {
            return NotFound(new
            {
                message =
                    "Report not found or the report could not be reviewed."
            });
        }

        return Ok(new
        {
            message = "Report reviewed successfully.",
            reportID = reportId,
            status = request.Status.Trim(),
            reviewedByUserID = reviewedByUserId
        });
    }
}