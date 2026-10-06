using System.Security.Claims;

using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/admin/profile")]
[Authorize(Policy = "ADMIN_PROFILE")]
public class AdminProfileController : ControllerBase
{
    private readonly IAdminProfileService _adminProfileService;

    public AdminProfileController(
        IAdminProfileService adminProfileService)
    {
        _adminProfileService =
            adminProfileService;
    }

    // ============================================================
    // GET MY PROFILE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdValue,
                out var adminUserId))
        {
            return Unauthorized(
                new
                {
                    success = false,
                    message =
                        "Administrator UserID was not found in the authentication token."
                });
        }

        var profile =
            await _adminProfileService
                .GetProfileAsync(adminUserId);

        if (profile == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Administrator profile was not found."
                });
        }

        return Ok(
            new
            {
                success = true,
                data = profile
            });
    }

    // ============================================================
    // UPDATE MY PROFILE
    // ============================================================

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateAdminProfileRequest request)
    {
        if (request == null)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message =
                        "Profile update request is required."
                });
        }

        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdValue,
                out var adminUserId))
        {
            return Unauthorized(
                new
                {
                    success = false,
                    message =
                        "Administrator UserID was not found in the authentication token."
                });
        }

        var updatedProfile =
            await _adminProfileService
                .UpdateProfileAsync(
                    adminUserId,
                    request);

        if (updatedProfile == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Administrator profile was not found."
                });
        }

        return Ok(
            new
            {
                success = true,
                message =
                    "Administrator profile updated successfully.",
                data = updatedProfile
            });
    }

    // ============================================================
    // REVOKE SESSION
    // ============================================================

    [HttpDelete("sessions/{sessionId:long}")]
    public async Task<IActionResult> RevokeSession(
        long sessionId)
    {
        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdValue,
                out var adminUserId))
        {
            return Unauthorized(
                new
                {
                    success = false,
                    message =
                        "Administrator UserID was not found in the authentication token."
                });
        }

        var revoked =
            await _adminProfileService
                .RevokeSessionAsync(
                    adminUserId,
                    sessionId);

        if (!revoked)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Session was not found or does not belong to the administrator."
                });
        }

        return Ok(
            new
            {
                success = true,
                message =
                    "Administrator session revoked successfully."
            });
    }
}