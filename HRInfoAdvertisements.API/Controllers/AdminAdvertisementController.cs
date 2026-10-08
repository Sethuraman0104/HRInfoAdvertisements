using System.Security.Claims;

using HRInfoAdvertisements.Application.DTOs.Admin;
using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/admin/advertisements")]
[Authorize]
public class AdminAdvertisementController : ControllerBase
{
    private readonly IAdvertisementModerationService _service;
private readonly IAdvertisementRemovalRequestService _removalRequestService;

public AdminAdvertisementController(
    IAdvertisementModerationService service,
    IAdvertisementRemovalRequestService removalRequestService)
{
    _service = service;
    _removalRequestService = removalRequestService;
}

    // ============================================================
    // GET LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAdvertisements(
        [FromQuery] string? statusCode,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result =
                await _service.GetAdvertisementsAsync(
                    statusCode,
                    search,
                    pageNumber,
                    pageSize);

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
    // GET DETAIL
    // ============================================================

    [HttpGet("{advertisementId:long}")]
    public async Task<IActionResult> GetAdvertisement(
        long advertisementId)
    {
        try
        {
            var result =
                await _service.GetAdvertisementAsync(
                    advertisementId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Advertisement not found."
                });
            }

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
// UPDATE ADVERTISEMENT
// ADMIN CAN UPDATE ANY ADVERTISEMENT
// ============================================================

[HttpPut("{advertisementId:long}")]
public async Task<IActionResult> UpdateAdvertisement(
    long advertisementId,
    [FromBody] UpdateAdvertisementRequest request)
{
    if (!TryGetUserId(out var adminUserId))
    {
        return Unauthorized(new
        {
            Success = false,
            Message = "Invalid administrator identity."
        });
    }

    if (!ModelState.IsValid)
    {
        return ValidationProblem(ModelState);
    }

    try
    {
        var result =
            await _service.UpdateAdvertisementAsync(
                adminUserId,
                advertisementId,
                request);

        if (result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Advertisement not found."
            });
        }

        return Ok(result);
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
    // APPROVE
    // ============================================================

    [HttpPost("{advertisementId:long}/approve")]
    public async Task<IActionResult> ApproveAdvertisement(
        long advertisementId,
        [FromBody] ApproveAdvertisementRequest request)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid administrator identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result =
                await _service.ApproveAdvertisementAsync(
                    adminUserId,
                    advertisementId,
                    request);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Advertisement not found or could not be approved."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement approved and published successfully."
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
    }

    // ============================================================
    // REJECT
    // ============================================================

    [HttpPost("{advertisementId:long}/reject")]
    public async Task<IActionResult> RejectAdvertisement(
        long advertisementId,
        [FromBody] RejectAdvertisementRequest request)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid administrator identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result =
                await _service.RejectAdvertisementAsync(
                    adminUserId,
                    advertisementId,
                    request);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Advertisement not found or could not be rejected."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement rejected successfully."
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
    }

    // ============================================================
    // SUSPEND
    // ============================================================

    [HttpPost("{advertisementId:long}/suspend")]
    public async Task<IActionResult> SuspendAdvertisement(
        long advertisementId,
        [FromBody] SuspendAdvertisementRequest request)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid administrator identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result =
                await _service.SuspendAdvertisementAsync(
                    adminUserId,
                    advertisementId,
                    request);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Advertisement not found or could not be suspended."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement suspended successfully."
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
    }

    // ============================================================
    // REACTIVATE
    // ============================================================

    [HttpPost("{advertisementId:long}/reactivate")]
    public async Task<IActionResult> ReactivateAdvertisement(
        long advertisementId,
        [FromBody] string? comments)
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
                await _service.ReactivateAdvertisementAsync(
                    adminUserId,
                    advertisementId,
                    comments);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Advertisement not found or could not be reactivated."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement reactivated successfully."
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
    }

    // ============================================================
    // APPROVAL HISTORY
    // ============================================================

    [HttpGet("{advertisementId:long}/approval-history")]
    public async Task<IActionResult> GetApprovalHistory(
        long advertisementId)
    {
        try
        {
            var result =
                await _service.GetApprovalHistoryAsync(
                    advertisementId);

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
    // REJECTION REASONS
    // ============================================================

    [HttpGet("rejection-reasons")]
    public async Task<IActionResult> GetRejectionReasons()
    {
        try
        {
            var result =
                await _service.GetRejectionReasonsAsync();

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

    // ============================================================
// PENDING REMOVAL REQUESTS
// ============================================================

[HttpGet("removal-requests")]
public async Task<IActionResult> GetPendingRemovalRequests(
    CancellationToken cancellationToken)
{
    try
    {
        var result =
            await _removalRequestService.GetPendingForAdminAsync(
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

[HttpPost("removal-requests/{removalRequestId:long}/approve")]
public async Task<IActionResult> ApproveRemovalRequest(
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
            await _removalRequestService.ApproveAsync(
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
                    "Removal request not found or could not be approved."
            });
        }

        return Ok(new
        {
            Success = true,
            Message =
                "Advertisement removed from the public listing successfully."
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
}
// ============================================================
// REJECT REMOVAL REQUEST
// ============================================================

[HttpPost("removal-requests/{removalRequestId:long}/reject")]
public async Task<IActionResult> RejectRemovalRequest(
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
            await _removalRequestService.RejectAsync(
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
                    "Removal request not found or could not be rejected."
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
}
}
