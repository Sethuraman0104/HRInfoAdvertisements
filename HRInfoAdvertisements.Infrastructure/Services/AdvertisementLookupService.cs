using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementLookupService
    : IAdvertisementLookupService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementLookupService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LookupItemResponse>>
        GetCategoriesAsync()
    {
        return await _context.AdvertisementCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .Select(x => new LookupItemResponse
            {
                ID = x.CategoryID,
                Name = x.CategoryName,
                NameAr = x.CategoryNameAr
            })
            .ToListAsync();
    }

    public async Task<List<LookupItemResponse>>
        GetAdvertisementTypesAsync()
    {
        return await _context.AdvertisementTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.TypeName)
            .Select(x => new LookupItemResponse
            {
                ID = x.AdvertisementTypeID,
                Name = x.TypeName,
                NameAr = x.TypeNameAr
            })
            .ToListAsync();
    }

    public async Task<List<LookupItemResponse>>
        GetCountriesAsync()
    {
        return await _context.Countries
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.CountryName)
            .Select(x => new LookupItemResponse
            {
                ID = x.CountryID,
                Name = x.CountryName,
                NameAr = x.CountryNameAr
            })
            .ToListAsync();
    }

    public async Task<List<LookupItemResponse>>
        GetStatesAsync(int countryId)
    {
        return await _context.States
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.CountryID == countryId)
            .OrderBy(x => x.StateName)
            .Select(x => new LookupItemResponse
            {
                ID = x.StateID,
                Name = x.StateName,
                NameAr = x.StateNameAr
            })
            .ToListAsync();
    }

    public async Task<List<LookupItemResponse>>
        GetCitiesAsync(
            int countryId,
            int? stateId = null)
    {
        var query = _context.Cities
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (stateId.HasValue)
        {
            query = query.Where(x =>
                x.StateID == stateId.Value);
        }
        else
        {
            // If no state is selected, return cities
            // belonging to states in the selected country.
            query = query.Where(x =>
                x.State != null &&
                x.State.CountryID == countryId);
        }

        return await query
            .OrderBy(x => x.CityName)
            .Select(x => new LookupItemResponse
            {
                ID = x.CityID,
                Name = x.CityName,
                NameAr = x.CityNameAr
            })
            .ToListAsync();
    }

    public async Task<List<LookupItemResponse>>
    GetAreasAsync(
        int countryId,
        int? stateId = null,
        int? cityId = null)
{
    var query = _context.Areas
        .AsNoTracking()
        .Where(x => x.IsActive);

    if (cityId.HasValue)
    {
        query = query.Where(x =>
            x.CityID == cityId.Value);
    }
    else if (stateId.HasValue)
    {
        query = query.Where(x =>
            x.City != null &&
            x.City.StateID == stateId.Value);
    }
    else
    {
        query = query.Where(x =>
            x.City != null &&
            x.City.State != null &&
            x.City.State.CountryID == countryId);
    }

    return await query
        .OrderBy(x => x.AreaName)
        .Select(x => new LookupItemResponse
        {
            ID = x.AreaID,
            Name = x.AreaName,
            NameAr = x.AreaNameAr
        })
        .ToListAsync();
}
}