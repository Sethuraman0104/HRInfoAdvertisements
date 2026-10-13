using HRInfoAdvertisements.Application.Billing;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly IEnquiryBillingService _billingService;

    public BillingController(IEnquiryBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("packages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePackages(
        CancellationToken cancellationToken)
    {
        var packages = await _billingService.GetAvailablePackagesAsync(
            cancellationToken);

        return Ok(packages);
    }
}
