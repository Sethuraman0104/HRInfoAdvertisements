using System.Security.Claims;

using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportSubmissionController : ControllerBase
{
    private readonly IReportSubmissionService _service;

    public ReportSubmissionController(
        IReportSubmissionService service)
    {
        _service = service;
    }

    // ============================================================
    // CREATE REPORT
    // ============================================================

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var reportId =
            await _service.CreateReportAsync(
                userId,
                request);

        if (reportId == null)
        {
            return BadRequest(new
            {
                Success = false,
                Message =
                    "Report could not be created. Please verify that the advertisement and report reason are valid, and that you have not already reported this advertisement."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = "Report submitted successfully.",
            ReportID = reportId.Value
        });
    }

    // ============================================================
    // GET USER ID FROM JWT
    // ============================================================

    private bool TryGetUserId(
        out long userId)
    {
        userId = 0;

        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(
            userIdClaim,
            out userId);
    }
}