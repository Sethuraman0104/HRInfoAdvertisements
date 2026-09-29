using HRInfoAdvertisements.Application.DTOs.SystemSettings;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface ISystemSettingService
{
    Task<SystemSettingListResponse> GetSystemSettingsAsync(
        SystemSettingListRequest request);

    Task<SystemSettingDetailResponse?> GetSystemSettingByIdAsync(
        int systemSettingId);

    Task<int?> CreateSystemSettingAsync(
        CreateSystemSettingRequest request,
        long createdBy);

    Task<bool> UpdateSystemSettingAsync(
        int systemSettingId,
        UpdateSystemSettingRequest request,
        long modifiedBy);

    Task<bool> UpdateSystemSettingStatusAsync(
        int systemSettingId,
        bool isActive,
        long modifiedBy);
}