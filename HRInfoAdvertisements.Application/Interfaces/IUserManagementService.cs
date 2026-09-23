using HRInfoAdvertisements.Application.DTOs.UserManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserListResponse> GetUsersAsync(
        UserListRequest request);

    Task<UserDetailResponse?> GetUserByIdAsync(
        long userId);

    Task<bool> UpdateUserStatusAsync(
        long userId,
        string accountStatus,
        long modifiedBy);

    Task<bool> UpdateUserLockAsync(
        long userId,
        bool isLocked,
        DateTime? lockoutEndDate,
        long modifiedBy);

    Task<bool> AssignRoleAsync(
        long userId,
        int roleId,
        long modifiedBy);

    Task<bool> RemoveRoleAsync(
        long userId,
        int roleId,
        long modifiedBy);
}