using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdminProfileService
{
    Task<AdminProfileResponse?> GetProfileAsync(
        long adminUserId);

    Task<AdminProfileResponse?> UpdateProfileAsync(
        long adminUserId,
        UpdateAdminProfileRequest request);

    Task<bool> RevokeSessionAsync(
        long adminUserId,
        long sessionId);
}