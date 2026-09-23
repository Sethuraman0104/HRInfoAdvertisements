using System.Security.Claims;

using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/favorites")]
[Authorize]
public class AdvertisementFavoriteController
    : ControllerBase
{
    private readonly IAdvertisementFavoriteService
        _favoriteService;

    public AdvertisementFavoriteController(
        IAdvertisementFavoriteService favoriteService)
    {
        _favoriteService =
            favoriteService;
    }

    // --------------------------------------------------
    // Add Favorite
    // --------------------------------------------------

    [HttpPost("{advertisementId:long}")]
    public async Task<IActionResult> AddFavorite(
        long advertisementId)
    {
        var userId = GetUserId();

        var result =
            await _favoriteService
                .AddFavoriteAsync(
                    userId,
                    advertisementId);

        if (!result)
        {
            return NotFound(
                new
                {
                    message =
                        "Advertisement not found."
                });
        }

        return Ok(
            new
            {
                message =
                    "Advertisement added to favorites."
            });
    }

    // --------------------------------------------------
    // Remove Favorite
    // --------------------------------------------------

    [HttpDelete("{advertisementId:long}")]
    public async Task<IActionResult> RemoveFavorite(
        long advertisementId)
    {
        var userId = GetUserId();

        var result =
            await _favoriteService
                .RemoveFavoriteAsync(
                    userId,
                    advertisementId);

        if (!result)
        {
            return NotFound(
                new
                {
                    message =
                        "Favorite was not found."
                });
        }

        return Ok(
            new
            {
                message =
                    "Advertisement removed from favorites."
            });
    }

    // --------------------------------------------------
    // Check Favorite
    // --------------------------------------------------

    [HttpGet("{advertisementId:long}/status")]
    public async Task<IActionResult> GetFavoriteStatus(
        long advertisementId)
    {
        var userId = GetUserId();

        var result =
            await _favoriteService
                .GetFavoriteStatusAsync(
                    userId,
                    advertisementId);

        return Ok(result);
    }

    // --------------------------------------------------
    // My Favorites
    // --------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetUserId();

        var result =
            await _favoriteService
                .GetMyFavoritesAsync(
                    userId);

        return Ok(result);
    }

    // --------------------------------------------------
    // Current User
    // --------------------------------------------------

    private long GetUserId()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            throw new UnauthorizedAccessException(
                "User ID was not found in the authentication token.");
        }

        if (!long.TryParse(
                claim.Value,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user ID in authentication token.");
        }

        return userId;
    }
}