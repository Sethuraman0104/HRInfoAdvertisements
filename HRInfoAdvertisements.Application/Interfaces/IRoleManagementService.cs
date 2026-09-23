using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IRoleManagementService
{
    Task<List<RoleListResponse>> GetRolesAsync();

    Task<RoleDetailResponse?> GetRoleByIdAsync(int roleId);

    Task<int?> CreateRoleAsync(
        string roleName,
        string? roleDescription,
        long createdBy);

    Task<bool> UpdateRoleAsync(
        int roleId,
        string roleName,
        string? roleDescription,
        long modifiedBy);

    Task<bool> UpdateRoleStatusAsync(
        int roleId,
        bool isActive,
        long modifiedBy);
}