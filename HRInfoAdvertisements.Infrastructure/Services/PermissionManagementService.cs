using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class PermissionManagementService : IPermissionManagementService
{
    private readonly ApplicationDbContext _context;

    public PermissionManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionListResponse>> GetPermissionsAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .OrderBy(x => x.PermissionCode)
            .Select(x => new PermissionListResponse
            {
                PermissionID = x.PermissionID,
                PermissionCode = x.PermissionCode,
                PermissionName = x.PermissionName,
                Description = x.Description,
                IsActive = x.IsActive,
                RoleCount = x.RolePermissions.Count()
            })
            .ToListAsync();
    }

    public async Task<PermissionDetailResponse?> GetPermissionByIdAsync(
        int permissionId)
    {
        var permission = await _context.Permissions
            .AsNoTracking()
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.PermissionID == permissionId);

        if (permission == null)
            return null;

        return new PermissionDetailResponse
        {
            PermissionID = permission.PermissionID,
            PermissionCode = permission.PermissionCode,
            PermissionName = permission.PermissionName,
            Description = permission.Description,
            IsActive = permission.IsActive,

            Roles = permission.RolePermissions
                .Where(x => x.Role != null)
                .OrderBy(x => x.Role.RoleName)
                .Select(x => new PermissionRoleResponse
                {
                    RoleID = x.RoleID,
                    RoleName = x.Role.RoleName,
                    IsActive = x.Role.IsActive
                })
                .ToList()
        };
    }
}