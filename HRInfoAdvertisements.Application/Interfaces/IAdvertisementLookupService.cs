using HRInfoAdvertisements.Application.DTOs.Advertisements;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementLookupService
{
    Task<List<LookupItemResponse>> GetCategoriesAsync();

    Task<List<LookupItemResponse>> GetAdvertisementTypesAsync();

    Task<List<LookupItemResponse>> GetCountriesAsync();

    Task<List<LookupItemResponse>> GetStatesAsync(
        int countryId);

    Task<List<LookupItemResponse>> GetCitiesAsync(
        int countryId,
        int? stateId = null);

    Task<List<LookupItemResponse>> GetAreasAsync(
        int countryId,
        int? stateId = null,
        int? cityId = null);
}