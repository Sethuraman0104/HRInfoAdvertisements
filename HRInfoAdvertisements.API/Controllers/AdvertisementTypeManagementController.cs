using HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/advertisement-types")]
[Authorize]
public class AdvertisementTypeManagementController
    : ControllerBase
{
    private readonly IAdvertisementTypeManagementService
        _advertisementTypeManagementService;

    public AdvertisementTypeManagementController(
        IAdvertisementTypeManagementService
            advertisementTypeManagementService)
    {
        _advertisementTypeManagementService =
            advertisementTypeManagementService;
    }

    // ------------------------------------------------------------
    // Get Advertisement Types
    // ------------------------------------------------------------

    [HttpGet]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetAdvertisementTypes(
        [FromQuery] AdvertisementTypeListRequest request)
    {
        var result =
            await _advertisementTypeManagementService
                .GetAdvertisementTypesAsync(request);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Get Advertisement Type
    // ------------------------------------------------------------

    [HttpGet("{advertisementTypeId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetAdvertisementType(
        int advertisementTypeId)
    {
        var result =
            await _advertisementTypeManagementService
                .GetAdvertisementTypeByIdAsync(
                    advertisementTypeId);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Advertisement type not found."
            });
        }

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Create Advertisement Type
    // ------------------------------------------------------------

    [HttpPost]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> CreateAdvertisementType(
        [FromBody] CreateAdvertisementTypeRequest request)
    {
        var createdBy = GetCurrentUserId();

        if (createdBy == null)
            return Unauthorized();

        var advertisementTypeId =
            await _advertisementTypeManagementService
                .CreateAdvertisementTypeAsync(
                    request,
                    createdBy.Value);

        if (advertisementTypeId == null)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to create advertisement type. " +
                    "The type name may already exist."
            });
        }

        var result =
            await _advertisementTypeManagementService
                .GetAdvertisementTypeByIdAsync(
                    advertisementTypeId.Value);

        return CreatedAtAction(
            nameof(GetAdvertisementType),
            new
            {
                advertisementTypeId =
                    advertisementTypeId.Value
            },
            result);
    }

    // ------------------------------------------------------------
    // Update Advertisement Type
    // ------------------------------------------------------------

    [HttpPut("{advertisementTypeId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateAdvertisementType(
        int advertisementTypeId,
        [FromBody] UpdateAdvertisementTypeRequest request)
    {
        var modifiedBy = GetCurrentUserId();

        if (modifiedBy == null)
            return Unauthorized();

        var updated =
            await _advertisementTypeManagementService
                .UpdateAdvertisementTypeAsync(
                    advertisementTypeId,
                    request,
                    modifiedBy.Value);

        if (!updated)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update advertisement type. " +
                    "The type may not exist or " +
                    "the type name may already be in use."
            });
        }

        var result =
            await _advertisementTypeManagementService
                .GetAdvertisementTypeByIdAsync(
                    advertisementTypeId);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Update Status
    // ------------------------------------------------------------

    [HttpPut("{advertisementTypeId:int}/status")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateAdvertisementTypeStatus(
        int advertisementTypeId,
        [FromBody] UpdateAdvertisementTypeStatusRequest request)
    {
        var modifiedBy = GetCurrentUserId();

        if (modifiedBy == null)
            return Unauthorized();

        var updated =
            await _advertisementTypeManagementService
                .UpdateAdvertisementTypeStatusAsync(
                    advertisementTypeId,
                    request.IsActive,
                    modifiedBy.Value);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Advertisement type not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Advertisement type activated successfully."
                : "Advertisement type deactivated successfully."
        });
    }

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