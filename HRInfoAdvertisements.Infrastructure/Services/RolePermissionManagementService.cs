using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class RolePermissionManagementService : IRolePermissionManagementService
{
    private readonly ApplicationDbContext _context;

    public RolePermissionManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RolePermissionDetailResponse?> GetRolePermissionsAsync(
        int roleId)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.RoleID == roleId);

        if (role == null)
            return null;

        return new RolePermissionDetailResponse
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            IsActive = role.IsActive,

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

    public async Task<bool> AssignPermissionAsync(
    int roleId,
    int permissionId,
    long modifiedBy)
{
    var role = await _context.Roles
        .FirstOrDefaultAsync(x => x.RoleID == roleId);

    if (role == null)
        return false;

    if (!role.IsActive)
        return false;

    var permission = await _context.Permissions
        .FirstOrDefaultAsync(x => x.PermissionID == permissionId);

    if (permission == null)
        return false;

    if (!permission.IsActive)
        return false;

    var alreadyAssigned = await _context.RolePermissions
        .AnyAsync(x =>
            x.RoleID == roleId &&
            x.PermissionID == permissionId);

    if (alreadyAssigned)
        return false;

    var rolePermission = new RolePermission
    {
        RoleID = roleId,
        PermissionID = permissionId,
        CreatedDate = DateTime.UtcNow
    };

    _context.RolePermissions.Add(rolePermission);

    await _context.SaveChangesAsync();

    return true;
}

public async Task<bool> RemovePermissionAsync(
    int roleId,
    int permissionId,
    long modifiedBy)
{
    var role = await _context.Roles
        .FirstOrDefaultAsync(x => x.RoleID == roleId);

    if (role == null)
        return false;

    if (!role.IsActive)
        return false;

    var rolePermission = await _context.RolePermissions
        .FirstOrDefaultAsync(x =>
            x.RoleID == roleId &&
            x.PermissionID == permissionId);

    if (rolePermission == null)
        return false;

    // SuperAdmin must retain all system permissions.
    if (string.Equals(
        role.RoleName,
        "SuperAdmin",
        StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    _context.RolePermissions.Remove(rolePermission);

    await _context.SaveChangesAsync();

    return true;
}
}