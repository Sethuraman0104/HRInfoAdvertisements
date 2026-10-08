using System.Security.Claims;

using HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/advertisements/{advertisementId:long}/removal-request")]
public class AdvertisementRemovalRequestsController
    : ControllerBase
{
    private readonly IAdvertisementRemovalRequestService _service;

    public AdvertisementRemovalRequestsController(
        IAdvertisementRemovalRequestService service)
    {
        _service = service;
    }

    private long? CurrentUserId =>
        long.TryParse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            out var id)
            ? id
            : null;

    // ------------------------------------------------------------
    // GET
    // Get the latest removal request for the current advertiser.
    // Returns 204 when there is no request.
    // ------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Get(
        long advertisementId,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not long userId)
        {
            return Unauthorized();
        }

        var result =
            await _service.GetLatestAsync(
                userId,
                advertisementId,
                cancellationToken);

        return result is null
            ? NoContent()
            : Ok(result);
    }

    // ------------------------------------------------------------
    // POST
    // Create a removal / unpublish request.
    // ------------------------------------------------------------

    [HttpPost]
    public async Task<IActionResult> Create(
        long advertisementId,
        [FromBody]
        CreateAdvertisementRemovalRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not long userId)
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _service.CreateAsync(
                    userId,
                    advertisementId,
                    request,
                    cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                });
        }
    }

    // ------------------------------------------------------------
    // DELETE
    // Cancel a pending removal request.
    // ------------------------------------------------------------

    [HttpDelete]
    public async Task<IActionResult> Cancel(
        long advertisementId,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not long userId)
        {
            return Unauthorized();
        }

        var cancelled =
            await _service.CancelPendingAsync(
                userId,
                advertisementId,
                cancellationToken);

        return cancelled
            ? NoContent()
            : NotFound(
                new
                {
                    message =
                        "There is no pending request to cancel."
                });
    }
}