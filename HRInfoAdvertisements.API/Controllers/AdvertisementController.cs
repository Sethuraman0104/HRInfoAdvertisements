using System.Security.Claims;

using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/advertisements")]
public class AdvertisementController : ControllerBase
{
    private readonly IAdvertisementService _advertisementService;

    public AdvertisementController(
        IAdvertisementService advertisementService)
    {
        _advertisementService = advertisementService;
    }

    // ============================================================
    // GET PUBLISHED ADVERTISEMENTS
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdvertisements(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? typeId = null,
        [FromQuery] int? countryId = null,
        [FromQuery] int? stateId = null,
        [FromQuery] int? cityId = null,
        [FromQuery] int? areaId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        var result =
            await _advertisementService
                .GetPublishedAdvertisementsAsync(
                    pageNumber,
                    pageSize,
                    search,
                    categoryId,
                    typeId,
                    countryId,
                    stateId,
                    cityId,
                    areaId,
                    minPrice,
                    maxPrice);

        return Ok(result);
    }

[HttpGet("public/{advertisementId:long}")]
[AllowAnonymous]
public async Task<IActionResult> GetPublicAdvertisement(
    long advertisementId)
{
    var advertisement =
        await _advertisementService
            .GetPublicAdvertisementByIdAsync(
                advertisementId);

    if (advertisement is null)
    {
        return NotFound();
    }

    return Ok(advertisement);
}
    // ============================================================
    // GET MY ADVERTISEMENTS
    // ============================================================

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyAdvertisements(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result =
            await _advertisementService
                .GetMyAdvertisementsAsync(
                    userId,
                    pageNumber,
                    pageSize);

        return Ok(result);
    }

    // ============================================================
    // GET ADVERTISEMENT BY ID
    // ============================================================

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        long id)
    {
        long? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            if (TryGetUserId(out var authenticatedUserId))
            {
                userId = authenticatedUserId;
            }
        }

        /*
         * IMPORTANT:
         *
         * The service must return contact information according
         * to the caller's context.
         *
         * Public visitor:
         *   WhatsAppNumber is returned only when
         *   ShowWhatsAppToPublic = true.
         *
         *   ContactEmail is returned only when
         *   ShowEmailToPublic = true.
         *
         * Advertisement owner:
         *   Full contact information may be returned.
         *
         * Admin:
         *   Full contact information may be returned through
         *   the Admin advertisement service.
         */

        var result =
            await _advertisementService
                .GetByIdAsync(
                    id,
                    userId);

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

    // ============================================================
    // SET PRIMARY IMAGE
    // ============================================================

    [HttpPut("{advertisementId:long}/media/images/{imageId:long}/primary")]
    [Authorize]
    public async Task<IActionResult> SetPrimaryImage(
        long advertisementId,
        long imageId,
        [FromServices]
        IAdvertisementMediaService mediaService)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        try
        {
            var result =
                await mediaService.SetPrimaryImageAsync(
                    userId,
                    advertisementId,
                    imageId);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message =
                        "Advertisement image not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Main advertisement photo updated successfully."
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
    // CREATE ADVERTISEMENT
    // ============================================================

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdvertisementRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            /*
             * Contact information is part of the advertisement
             * request.
             *
             * The service is responsible for saving:
             *
             *   WhatsAppNumber
             *   ContactEmail
             *   ShowWhatsAppToPublic
             *   ShowEmailToPublic
             */

            var result =
                await _advertisementService
                    .CreateAsync(
                        userId,
                        request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = result.AdvertisementID
                },
                result);
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
    // UPDATE ADVERTISEMENT
    // ============================================================

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateAdvertisementRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            /*
             * Update includes the advertisement contact settings.
             *
             * Only the owner can update these values.
             */

            var result =
                await _advertisementService
                    .UpdateAsync(
                        userId,
                        id,
                        request);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message =
                        "Advertisement not found or you are not the owner."
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
    }

    // ============================================================
    // DELETE ADVERTISEMENT
    // ============================================================

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result =
            await _advertisementService
                .DeleteAsync(
                    userId,
                    id);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message =
                    "Advertisement could not be deleted. Only Draft or Rejected advertisements can be deleted."
            });
        }

        return Ok(new
        {
            Success = true,
            Message =
                "Advertisement deleted successfully."
        });
    }

    // ============================================================
    // SUBMIT ADVERTISEMENT
    // ============================================================

    [HttpPost("{id:long}/submit")]
    [Authorize]
    public async Task<IActionResult> Submit(
        long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        try
        {
            /*
             * SubmitAsync should validate the advertisement,
             * including the contact information rules.
             *
             * Recommended rules:
             *
             * 1. Draft can exist without contact information.
             * 2. Submission requires valid contact information.
             * 3. At least one contact method must be enabled
             *    for public display.
             */

            var result =
                await _advertisementService
                    .SubmitAsync(
                        userId,
                        id);

            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Advertisement could not be submitted. Please make sure it exists, belongs to you, is in Draft status, contains the required information, and has valid public contact settings."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Advertisement submitted for approval successfully."
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
    // GET USER ID FROM JWT
    // ============================================================

    private bool TryGetUserId(
        out long userId)
    {
        userId = 0;

        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(
            userIdClaim,
            out userId);
    }
}