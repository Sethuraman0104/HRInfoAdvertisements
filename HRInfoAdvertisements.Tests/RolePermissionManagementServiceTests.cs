using System.Text.Json;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HRInfoAdvertisements.Tests;

public class RolePermissionManagementServiceTests
{
    private static Task<(ApplicationDbContext Context,
        RolePermissionManagementService Service)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings =>
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ApplicationDbContext(options);

        return Task.FromResult((
            context,
            new RolePermissionManagementService(context)));
    }

    private static Role CreateRole(
        int id,
        string name,
        bool isActive = true)
        => new()
        {
            RoleID = id,
            RoleName = name,
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        };

    private static Permission CreatePermission(
        int id,
        string code,
        bool isActive = true)
        => new()
        {
            PermissionID = id,
            PermissionCode = code,
            PermissionName = code,
            IsActive = isActive
        };

    [Fact]
    public async Task AssignPermissionAsync_RejectsSuperAdmin()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(1, "SuperAdmin"));
            context.Permissions.Add(CreatePermission(1, "USER_VIEW"));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(1, 1, 100);

            Assert.False(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("SUPERADMIN")]
    [InlineData("superadmin")]
    public async Task RemovePermissionAsync_RejectsSuperAdminRegardlessOfCase(
        string roleName)
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            var role = CreateRole(1, roleName);
            var permission = CreatePermission(1, "USER_VIEW");

            context.Roles.Add(role);
            context.Permissions.Add(permission);
            context.RolePermissions.Add(new RolePermission
            {
                RolePermissionID = 1,
                RoleID = 1,
                PermissionID = 1,
                CreatedDate = DateTime.UtcNow,
                Role = role,
                Permission = permission
            });

            await context.SaveChangesAsync();

            var result = await service.RemovePermissionAsync(1, 1, 100);

            Assert.False(result);
            Assert.Single(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_RejectsRolePermissionManagementDelegation()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(2, "Admin"));
            context.Permissions.Add(
                CreatePermission(5, "ROLE_PERMISSION_MANAGE"));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 5, 100);

            Assert.False(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_RejectsDelegationRegardlessOfPermissionCodeCase()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(2, "Admin"));
            context.Permissions.Add(
                CreatePermission(5, "role_permission_manage"));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 5, 100);

            Assert.False(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_RejectsInactiveRole()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(2, "Admin", isActive: false));
            context.Permissions.Add(CreatePermission(1, "USER_VIEW"));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 1, 100);

            Assert.False(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_RejectsInactivePermission()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(2, "Admin"));
            context.Permissions.Add(
                CreatePermission(1, "USER_VIEW", isActive: false));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 1, 100);

            Assert.False(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_RejectsDuplicateAssignment()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            var role = CreateRole(2, "Admin");
            var permission = CreatePermission(1, "USER_VIEW");

            context.Roles.Add(role);
            context.Permissions.Add(permission);
            context.RolePermissions.Add(new RolePermission
            {
                RolePermissionID = 10,
                RoleID = 2,
                PermissionID = 1,
                CreatedDate = DateTime.UtcNow,
                Role = role,
                Permission = permission
            });

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 1, 100);

            Assert.False(result);
            Assert.Single(await context.RolePermissions.ToListAsync());
            Assert.Empty(await context.AuditLogs.ToListAsync());
        }
    }

    [Fact]
    public async Task AssignPermissionAsync_CreatesAssignmentAndAuditLog()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            context.Roles.Add(CreateRole(2, "Admin"));
            context.Permissions.Add(CreatePermission(1, "USER_VIEW"));

            await context.SaveChangesAsync();

            var result = await service.AssignPermissionAsync(2, 1, 100);

            Assert.True(result);

            var assignment = Assert.Single(
                await context.RolePermissions.ToListAsync());

            var audit = Assert.Single(
                await context.AuditLogs.ToListAsync());

            Assert.Equal(2, assignment.RoleID);
            Assert.Equal(1, assignment.PermissionID);

            Assert.Equal(100, audit.UserID);
            Assert.Equal("ROLE_PERMISSION_ASSIGNED", audit.Action);
            Assert.Equal("RolePermission", audit.EntityName);
            Assert.Equal(
                assignment.RolePermissionID.ToString(),
                audit.EntityID);

            Assert.NotNull(audit.NewValues);

            using var document = JsonDocument.Parse(audit.NewValues!);

            Assert.Equal(
                2,
                document.RootElement.GetProperty("RoleID").GetInt32());

            Assert.Equal(
                "Admin",
                document.RootElement.GetProperty("RoleName").GetString());

            Assert.Equal(
                "USER_VIEW",
                document.RootElement.GetProperty("PermissionCode").GetString());

            Assert.Equal(
                100,
                document.RootElement.GetProperty("ModifiedBy").GetInt64());

            Assert.Null(audit.OldValues);
        }
    }

    [Fact]
    public async Task RemovePermissionAsync_RemovesAssignmentAndCreatesAuditLog()
    {
        var (context, service) = await CreateServiceAsync();

        await using (context)
        {
            var role = CreateRole(2, "Admin");
            var permission = CreatePermission(1, "USER_VIEW");
            var createdDate = DateTime.UtcNow.AddDays(-1);

            context.Roles.Add(role);
            context.Permissions.Add(permission);
            context.RolePermissions.Add(new RolePermission
            {
                RolePermissionID = 10,
                RoleID = 2,
                PermissionID = 1,
                CreatedDate = createdDate,
                Role = role,
                Permission = permission
            });

            await context.SaveChangesAsync();

            var result = await service.RemovePermissionAsync(2, 1, 100);

            Assert.True(result);
            Assert.Empty(await context.RolePermissions.ToListAsync());

            var audit = Assert.Single(
                await context.AuditLogs.ToListAsync());

            Assert.Equal(100, audit.UserID);
            Assert.Equal("ROLE_PERMISSION_REMOVED", audit.Action);
            Assert.Equal("RolePermission", audit.EntityName);
            Assert.Equal("10", audit.EntityID);
            Assert.Null(audit.NewValues);
            Assert.NotNull(audit.OldValues);

            using var document = JsonDocument.Parse(audit.OldValues!);

            Assert.Equal(
                "USER_VIEW",
                document.RootElement.GetProperty("PermissionCode").GetString());

            Assert.Equal(
                "Admin",
                document.RootElement.GetProperty("RoleName").GetString());
        }
    }
}