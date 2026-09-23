using System.Security.Claims;
using HRInfoAdvertisements.Application.Profile;
using HRInfoAdvertisements.Application.Profile.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    // ------------------------------------------------------------
    // Current User
    // ------------------------------------------------------------

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result =
                await _profileService.GetProfileAsync(userId.Value);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result =
                await _profileService.UpdateProfileAsync(
                    userId.Value,
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
    }

    // ------------------------------------------------------------
    // Addresses
    // ------------------------------------------------------------

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result =
            await _profileService.GetAddressesAsync(userId.Value);

        return Ok(result);
    }

    [HttpGet("addresses/{addressId:long}")]
    public async Task<IActionResult> GetAddress(
        long addressId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result =
            await _profileService.GetAddressAsync(
                userId.Value,
                addressId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Address was not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress(
        [FromBody] CreateUserAddressRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result =
                await _profileService.AddAddressAsync(
                    userId.Value,
                    request);

            return CreatedAtAction(
                nameof(GetAddress),
                new
                {
                    addressId = result.UserAddressID
                },
                result);
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
    }

    [HttpPut("addresses/{addressId:long}")]
    public async Task<IActionResult> UpdateAddress(
        long addressId,
        [FromBody] UpdateUserAddressRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result =
                await _profileService.UpdateAddressAsync(
                    userId.Value,
                    addressId,
                    request);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Address was not found."
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
    }

    [HttpDelete("addresses/{addressId:long}")]
    public async Task<IActionResult> DeleteAddress(
        long addressId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var deleted =
            await _profileService.DeleteAddressAsync(
                userId.Value,
                addressId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Address was not found."
            });
        }

        return NoContent();
    }

    [HttpPost("addresses/{addressId:long}/default")]
    public async Task<IActionResult> SetPrimaryAddress(
        long addressId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var updated =
            await _profileService.SetPrimaryAddressAsync(
                userId.Value,
                addressId);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Address was not found."
            });
        }

        var address =
            await _profileService.GetAddressAsync(
                userId.Value,
                addressId);

        return Ok(address);
    }

    // ------------------------------------------------------------
    // Current User Helper
    // ------------------------------------------------------------

    private long? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("UserID")?.Value;

        if (long.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}