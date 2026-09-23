using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/advertisements")]
public class AdvertisementSearchController : ControllerBase
{
    private readonly IAdvertisementSearchService _searchService;

    public AdvertisementSearchController(
        IAdvertisementSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] AdvertisementSearchRequest request)
    {
        var result =
            await _searchService.SearchAsync(request);

        return Ok(result);
    }
}