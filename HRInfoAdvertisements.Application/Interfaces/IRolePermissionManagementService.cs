using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IRolePermissionManagementService
{
    Task<RolePermissionDetailResponse?> GetRolePermissionsAsync(
        int roleId);

    Task<bool> AssignPermissionAsync(
        int roleId,
        int permissionId,
        long modifiedBy);

    Task<bool> RemovePermissionAsync(
        int roleId,
        int permissionId,
        long modifiedBy);
}