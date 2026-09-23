using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IPermissionManagementService
{
    Task<List<PermissionListResponse>> GetPermissionsAsync();

    Task<PermissionDetailResponse?> GetPermissionByIdAsync(
        int permissionId);
}