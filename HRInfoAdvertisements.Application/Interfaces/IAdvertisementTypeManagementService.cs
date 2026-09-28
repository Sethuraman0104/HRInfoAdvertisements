using HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementTypeManagementService
{
    Task<AdvertisementTypeListResponse> GetAdvertisementTypesAsync(
        AdvertisementTypeListRequest request);

    Task<AdvertisementTypeDetailResponse?> GetAdvertisementTypeByIdAsync(
        int advertisementTypeId);

    Task<int?> CreateAdvertisementTypeAsync(
        CreateAdvertisementTypeRequest request,
        long createdBy);

    Task<bool> UpdateAdvertisementTypeAsync(
        int advertisementTypeId,
        UpdateAdvertisementTypeRequest request,
        long modifiedBy);

    Task<bool> UpdateAdvertisementTypeStatusAsync(
        int advertisementTypeId,
        bool isActive,
        long modifiedBy);
}