using System.Security.Claims;
using HRInfoAdvertisements.Application.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/admin/package-purchase-requests")]
[Authorize(Policy = "ENQUIRY_VIEW")]
public sealed class AdminPackagePurchaseRequestsController : ControllerBase
{
    private readonly IEnquiryBillingService _billingService;

    public AdminPackagePurchaseRequestsController(
        IEnquiryBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPending(
        CancellationToken cancellationToken)
    {
        var result =
            await _billingService.GetPendingPurchaseRequestsForAdminAsync(
                cancellationToken);

        return Ok(new
        {
            Success = true,
            Data = result
        });
    }

    [HttpPost("{purchaseRequestId:long}/approve")]
    public Task<IActionResult> Approve(
        long purchaseRequestId,
        CancellationToken cancellationToken)
    {
        return Review(
            purchaseRequestId,
            approve: true,
            cancellationToken);
    }

    [HttpPost("{purchaseRequestId:long}/reject")]
    public Task<IActionResult> Reject(
        long purchaseRequestId,
        CancellationToken cancellationToken)
    {
        return Review(
            purchaseRequestId,
            approve: false,
            cancellationToken);
    }

    private async Task<IActionResult> Review(
        long purchaseRequestId,
        bool approve,
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
            var result = await _billingService.ReviewPurchaseRequestAsync(
                adminUserId,
                purchaseRequestId,
                approve,
                cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Purchase request not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = approve
                    ? "Purchase request approved for the next payment step. No payment was confirmed."
                    : "Purchase request rejected.",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;

        var value =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(value, out userId) && userId > 0;
    }
}
