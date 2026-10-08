using System.Security.Claims;

using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/admin/advertisement-removal-requests")]
[Authorize]
public class AdminAdvertisementRemovalRequestController
    : ControllerBase
{
    private readonly IAdvertisementRemovalRequestService _service;

    public AdminAdvertisementRemovalRequestController(
        IAdvertisementRemovalRequestService service)
    {
        _service = service;
    }

    // ============================================================
    // GET PENDING REMOVAL REQUESTS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetPendingRequests(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.GetPendingForAdminAsync(
                    cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ============================================================
    // APPROVE REMOVAL REQUEST
    // ============================================================

    [HttpPost("{removalRequestId:long}/approve")]
    public async Task<IActionResult> Approve(
        long removalRequestId,
        [FromBody] string? comments,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid administrator identity."
            });
        }

        try
        {
            var result =
                await _service.ApproveAsync(
                    adminUserId,
                    removalRequestId,
                    comments,
                    cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message =
                        "Removal request not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement removal request approved successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ============================================================
    // REJECT REMOVAL REQUEST
    // ============================================================

    [HttpPost("{removalRequestId:long}/reject")]
    public async Task<IActionResult> Reject(
        long removalRequestId,
        [FromBody] string? comments,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid administrator identity."
            });
        }

        try
        {
            var result =
                await _service.RejectAsync(
                    adminUserId,
                    removalRequestId,
                    comments,
                    cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message =
                        "Removal request not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement removal request rejected successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ============================================================
    // GET ADMIN USER ID FROM JWT
    // ============================================================

    private bool TryGetUserId(out long userId)
    {
        userId = 0;

        var value =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(
            value,
            out userId);
    }
}