
using System.Text.Json;
using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class RolePermissionManagementService
    : IRolePermissionManagementService
{
    private readonly ApplicationDbContext _context;

    public RolePermissionManagementService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RolePermissionDetailResponse?>
        GetRolePermissionsAsync(int roleId)
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

        if (role == null || !role.IsActive)
            return false;

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(
                x => x.PermissionID == permissionId);

        if (permission == null || !permission.IsActive)
            return false;

        // SuperAdmin permissions cannot be modified
        // through this service.
        if (string.Equals(
            role.RoleName,
            "SuperAdmin",
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Do not delegate permission-management authority.
        if (string.Equals(
            permission.PermissionCode,
            "ROLE_PERMISSION_MANAGE",
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var alreadyAssigned = await _context.RolePermissions
            .AnyAsync(x =>
                x.RoleID == roleId &&
                x.PermissionID == permissionId);

        if (alreadyAssigned)
            return false;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;

        var rolePermission = new RolePermission
        {
            RoleID = roleId,
            PermissionID = permissionId,
            CreatedDate = now
        };

        _context.RolePermissions.Add(rolePermission);

        // Save to generate RolePermissionID.
        // The transaction remains open.
        await _context.SaveChangesAsync();

        var auditLog = new AuditLog
        {
            UserID = modifiedBy,
            Action = "ROLE_PERMISSION_ASSIGNED",
            EntityName = "RolePermission",
            EntityID =
                rolePermission.RolePermissionID.ToString(),
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                rolePermission.RolePermissionID,
                rolePermission.RoleID,
                RoleName = role.RoleName,
                rolePermission.PermissionID,
                PermissionCode = permission.PermissionCode,
                ModifiedBy = modifiedBy
            }),
            CreatedDate = now
        };

        _context.AuditLogs.Add(auditLog);

        // If the audit insert fails, the transaction
        // will not commit the permission assignment.
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> RemovePermissionAsync(
        int roleId,
        int permissionId,
        long modifiedBy)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(x => x.RoleID == roleId);

        if (role == null || !role.IsActive)
            return false;

        var rolePermission = await _context.RolePermissions
            .Include(x => x.Permission)
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

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;

        // Capture the original values before deletion.
        var oldValues = new
        {
            rolePermission.RolePermissionID,
            rolePermission.RoleID,
            RoleName = role.RoleName,
            rolePermission.PermissionID,
            PermissionCode =
                rolePermission.Permission.PermissionCode,
            rolePermission.CreatedDate
        };

        var rolePermissionId =
            rolePermission.RolePermissionID.ToString();

        _context.RolePermissions.Remove(rolePermission);

        // Delete the assignment within the transaction.
        await _context.SaveChangesAsync();

        var auditLog = new AuditLog
        {
            UserID = modifiedBy,
            Action = "ROLE_PERMISSION_REMOVED",
            EntityName = "RolePermission",
            EntityID = rolePermissionId,
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = null,
            CreatedDate = now
        };

        _context.AuditLogs.Add(auditLog);

        // Commit only after the audit record is saved.
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }
}
