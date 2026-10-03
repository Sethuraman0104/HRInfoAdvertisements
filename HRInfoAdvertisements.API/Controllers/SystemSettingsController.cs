using HRInfoAdvertisements.Application.DTOs.SystemSettings;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/system-settings")]
[Authorize]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService
        _systemSettingService;

    public SystemSettingsController(
        ISystemSettingService systemSettingService)
    {
        _systemSettingService =
            systemSettingService;
    }

    // ============================================================
    // GET SYSTEM SETTINGS
    // ============================================================

    [HttpGet]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetSystemSettings(
        [FromQuery] SystemSettingListRequest request)
    {
        var result =
            await _systemSettingService
                .GetSystemSettingsAsync(request);

        return Ok(result);
    }

// ============================================================
// GET APPLICATION SETTINGS
// ============================================================

[HttpGet("application")]
[Authorize(Policy = "ADVERTISEMENT_VIEW")]
public async Task<IActionResult> GetApplicationSettings(
    CancellationToken cancellationToken)
{
    var result =
        await _systemSettingService
            .GetApplicationSettingsAsync(
                cancellationToken);

    return Ok(result);
}

    // ============================================================
    // GET SYSTEM SETTING
    // ============================================================

    [HttpGet("{systemSettingId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetSystemSetting(
        int systemSettingId)
    {
        var result =
            await _systemSettingService
                .GetSystemSettingByIdAsync(
                    systemSettingId);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "System setting not found."
            });
        }

        return Ok(result);
    }

    // ============================================================
    // CREATE SYSTEM SETTING
    // ============================================================

    [HttpPost]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> CreateSystemSetting(
        [FromBody] CreateSystemSettingRequest request)
    {
        var createdBy =
            GetCurrentUserId();

        if (createdBy == null)
        {
            return Unauthorized();
        }

        var systemSettingId =
            await _systemSettingService
                .CreateSystemSettingAsync(
                    request,
                    createdBy.Value);

        if (systemSettingId == null)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to create system setting. " +
                    "The setting key may already exist."
            });
        }

        var result =
            await _systemSettingService
                .GetSystemSettingByIdAsync(
                    systemSettingId.Value);

        return CreatedAtAction(
            nameof(GetSystemSetting),
            new
            {
                systemSettingId =
                    systemSettingId.Value
            },
            result);
    }

    // ============================================================
    // UPDATE SYSTEM SETTING
    // ============================================================

    [HttpPut("{systemSettingId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateSystemSetting(
        int systemSettingId,
        [FromBody] UpdateSystemSettingRequest request)
    {
        var modifiedBy =
            GetCurrentUserId();

        if (modifiedBy == null)
        {
            return Unauthorized();
        }

        var updated =
            await _systemSettingService
                .UpdateSystemSettingAsync(
                    systemSettingId,
                    request,
                    modifiedBy.Value);

        if (!updated)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update system setting. " +
                    "The setting may not exist or " +
                    "the setting key may already be in use."
            });
        }

        var result =
            await _systemSettingService
                .GetSystemSettingByIdAsync(
                    systemSettingId);

        return Ok(result);
    }

    // ============================================================
    // UPDATE STATUS
    // ============================================================

    [HttpPut("{systemSettingId:int}/status")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateSystemSettingStatus(
        int systemSettingId,
        [FromBody]
        UpdateSystemSettingStatusRequest request)
    {
        var modifiedBy =
            GetCurrentUserId();

        if (modifiedBy == null)
        {
            return Unauthorized();
        }

        var updated =
            await _systemSettingService
                .UpdateSystemSettingStatusAsync(
                    systemSettingId,
                    request.IsActive,
                    modifiedBy.Value);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "System setting not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "System setting activated successfully."
                : "System setting deactivated successfully."
        });
    }

    // ============================================================
    // CURRENT USER
    // ============================================================

    private long? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("UserID")?.Value;

        if (long.TryParse(
                userIdClaim,
                out var userId))
        {
            return userId;
        }

        return null;
    }
}