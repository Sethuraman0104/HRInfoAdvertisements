using System.Security.Claims;
using HRInfoAdvertisements.Application.Enquiries;
using HRInfoAdvertisements.Application.Enquiries.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/enquiries")]
public class AdvertisementEnquiryController : ControllerBase
{
    private readonly IAdvertisementEnquiryService _service;

    public AdvertisementEnquiryController(
        IAdvertisementEnquiryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdvertisementEnquiryRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var result = await _service.CreateAsync(
                userId,
                request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetSentAsync(userId);

        return Ok(result);
    }

    [HttpGet("received")]
    public async Task<IActionResult> GetReceived()
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetReceivedAsync(userId);

        return Ok(result);
    }

    [HttpGet("{enquiryId:long}")]
    public async Task<IActionResult> GetById(
        long enquiryId)
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetByIdAsync(
            userId,
            enquiryId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Enquiry not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("{enquiryId:long}/reply")]
    public async Task<IActionResult> Reply(
        long enquiryId,
        [FromBody] ReplyAdvertisementEnquiryRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var result = await _service.ReplyAsync(
                userId,
                enquiryId,
                request);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Enquiry not found."
                });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{enquiryId:long}/close")]
    public async Task<IActionResult> Close(
        long enquiryId)
    {
        var userId = GetCurrentUserId();

        var result = await _service.CloseAsync(
            userId,
            enquiryId);

        if (!result)
        {
            return NotFound(new
            {
                message = "Enquiry not found."
            });
        }

        return Ok(new
        {
            message = "Enquiry closed successfully."
        });
    }

    [HttpPost("{enquiryId:long}/read")]
    public async Task<IActionResult> MarkAsRead(
        long enquiryId)
    {
        var userId = GetCurrentUserId();

        var result = await _service.MarkMessagesAsReadAsync(
            userId,
            enquiryId);

        if (!result)
        {
            return NotFound(new
            {
                message = "Enquiry not found."
            });
        }

        return Ok(new
        {
            message = "Messages marked as read."
        });
    }

    private long GetCurrentUserId()
    {
        var claim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User ID could not be determined.");
        }

        return userId;
    }
}