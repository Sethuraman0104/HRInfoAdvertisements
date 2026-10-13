using System.Security.Claims;
using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/billing/purchase-requests")]
[Authorize]
public sealed class PackagePurchaseRequestsController : ControllerBase
{
    private readonly IEnquiryBillingService _billingService;

    public PackagePurchaseRequestsController(
        IEnquiryBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PackagePurchaseRequestResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePackagePurchaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        if (request.EnquiryPackageID <= 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "A valid enquiry package ID is required."
            });
        }

        try
        {
            var result = await _billingService.CreatePurchaseRequestAsync(
                userId,
                request.EnquiryPackageID,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                Success = true,
                Message = "Package purchase request created. Payment and activation are not yet confirmed.",
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

    [HttpGet("mine")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result = await _billingService.GetMyPurchaseRequestsAsync(
            userId,
            cancellationToken);

        return Ok(result);
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;

        var value =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("userId")?.Value ??
            User.FindFirst("UserID")?.Value;

        return long.TryParse(value, out userId) && userId > 0;
    }
}
