using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly ApplicationDbContext _context;

    public RoleManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleListResponse>> GetRolesAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(x => x.RoleID)
            .Select(x => new RoleListResponse
            {
                RoleID = x.RoleID,
                RoleName = x.RoleName,
                RoleDescription = x.RoleDescription,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,

                UserCount = x.UserRoles.Count(),

                PermissionCount = x.RolePermissions.Count()
            })
            .ToListAsync();
    }

    public async Task<RoleDetailResponse?> GetRoleByIdAsync(int roleId)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.RoleID == roleId);

        if (role == null)
            return null;

        return new RoleDetailResponse
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            RoleDescription = role.RoleDescription,
            IsActive = role.IsActive,
            CreatedDate = role.CreatedDate,

            UserCount = role.UserRoles.Count,

            Permissions = role.RolePermissions
                .Where(x => x.Permission != null)
                .OrderBy(x => x.Permission.PermissionCode)
                .Select(x => new RolePermissionResponse
                {
                    PermissionID = x.PermissionID,
                    PermissionCode = x.Permission.PermissionCode,
                    PermissionName = x.Permission.PermissionName,
                    Description = x.Permission.Description,
                    IsActive = x.Permission.IsActive
                })
                .ToList()
        };
    }

    public async Task<int?> CreateRoleAsync(
        string roleName,
        string? roleDescription,
        long createdBy)
    {
        roleName = roleName.Trim();

        if (string.IsNullOrWhiteSpace(roleName))
            return null;

        var exists = await _context.Roles
            .AnyAsync(x =>
                x.RoleName.ToLower() == roleName.ToLower());

        if (exists)
            return null;

        var role = new Role
        {
            RoleName = roleName,
            RoleDescription = string.IsNullOrWhiteSpace(roleDescription)
                ? null
                : roleDescription.Trim(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Roles.Add(role);

        await _context.SaveChangesAsync();

        return role.RoleID;
    }

    public async Task<bool> UpdateRoleAsync(
        int roleId,
        string roleName,
        string? roleDescription,
        long modifiedBy)
    {
        roleName = roleName.Trim();

        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        var role = await _context.Roles
            .FirstOrDefaultAsync(x => x.RoleID == roleId);

        if (role == null)
            return false;

        var duplicateName = await _context.Roles
            .AnyAsync(x =>
                x.RoleID != roleId &&
                x.RoleName.ToLower() == roleName.ToLower());

        if (duplicateName)
            return false;

        // Do not allow renaming the built-in SuperAdmin role.
        if (role.RoleName.Equals(
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase) &&
            !roleName.Equals(
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        role.RoleName = roleName;

        role.RoleDescription =
            string.IsNullOrWhiteSpace(roleDescription)
                ? null
                : roleDescription.Trim();

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateRoleStatusAsync(
        int roleId,
        bool isActive,
        long modifiedBy)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(x => x.RoleID == roleId);

        if (role == null)
            return false;

        // SuperAdmin must always remain active.
        if (role.RoleName.Equals(
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase) &&
            !isActive)
        {
            return false;
        }

        // Prevent deactivating a role that is currently assigned
        // to users. Users should be reassigned first.
        if (!isActive)
        {
            var assignedUserCount = await _context.UserRoles
                .CountAsync(x => x.RoleID == roleId);

            if (assignedUserCount > 0)
                return false;
        }

        role.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }
}