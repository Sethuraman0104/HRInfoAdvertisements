using HRInfoAdvertisements.Application.DTOs.AuditLogs;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(
        IAuditLogService service)
    {
        _service = service;
    }

    // ============================================================
    // GET AUDIT LOGS
    // ============================================================

    [HttpGet]
    [Authorize(Policy = "AUDIT_VIEW")]
    public async Task<ActionResult<AuditLogListResponse>> GetAuditLogs(
        [FromQuery] AuditLogListRequest request)
    {
        var result =
            await _service.GetAuditLogsAsync(request);

        return Ok(result);
    }
}