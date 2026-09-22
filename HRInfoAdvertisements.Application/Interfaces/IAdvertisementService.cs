using HRInfoAdvertisements.Application.DTOs.Advertisements;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementService
{
    Task<AdvertisementResponse> CreateAsync(
        long userId,
        CreateAdvertisementRequest request);

    Task<AdvertisementResponse?> GetByIdAsync(
        long advertisementId,
        long? userId = null);

    Task<AdvertisementListResponse> GetMyAdvertisementsAsync(
        long userId,
        int pageNumber = 1,
        int pageSize = 20);

    Task<AdvertisementListResponse> GetPublishedAdvertisementsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        int? categoryId = null,
        int? typeId = null,
        int? countryId = null,
        int? stateId = null,
        int? cityId = null,
        int? areaId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);

    Task<AdvertisementResponse?> UpdateAsync(
        long userId,
        long advertisementId,
        UpdateAdvertisementRequest request);

    Task<bool> DeleteAsync(
        long userId,
        long advertisementId);

    Task<bool> SubmitAsync(
        long userId,
        long advertisementId);
}