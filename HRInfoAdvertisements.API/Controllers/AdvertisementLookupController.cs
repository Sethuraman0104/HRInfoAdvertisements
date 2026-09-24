using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/advertisement-lookups")]
public class AdvertisementLookupController : ControllerBase
{
    private readonly IAdvertisementLookupService _lookupService;

    public AdvertisementLookupController(
        IAdvertisementLookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetCategories()
    {
        return Ok(
            await _lookupService.GetCategoriesAsync());
    }

    [HttpGet("types")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetAdvertisementTypes()
    {
        return Ok(
            await _lookupService
                .GetAdvertisementTypesAsync());
    }

    [HttpGet("countries")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetCountries()
    {
        return Ok(
            await _lookupService.GetCountriesAsync());
    }

    [HttpGet("states/{countryId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetStates(int countryId)
    {
        return Ok(
            await _lookupService
                .GetStatesAsync(countryId));
    }

    [HttpGet("cities/{countryId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetCities(
            int countryId,
            [FromQuery] int? stateId = null)
    {
        return Ok(
            await _lookupService
                .GetCitiesAsync(
                    countryId,
                    stateId));
    }

    [HttpGet("areas/{countryId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LookupItemResponse>>>
        GetAreas(
            int countryId,
            [FromQuery] int? stateId = null,
            [FromQuery] int? cityId = null)
    {
        return Ok(
            await _lookupService
                .GetAreasAsync(
                    countryId,
                    stateId,
                    cityId));
    }
}