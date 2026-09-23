using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context)
    {
        // ========================================================
        // APPLY PENDING MIGRATIONS
        // ========================================================

        await context.Database.MigrateAsync();

        // ========================================================
        // SECURITY / AUTHORIZATION MASTER DATA
        // ========================================================

        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context);

        // ========================================================
        // DEFAULT ADMINISTRATOR ACCOUNT
        // ========================================================

        await SeedAdminUserAsync(context);

        // ========================================================
        // ADVERTISEMENT MASTER DATA
        // ========================================================

        await SeedAdvertisementStatusesAsync(context);
        await SeedAdvertisementCategoriesAsync(context);
        await SeedAdvertisementTypesAsync(context);

        // ========================================================
        // LOCATION MASTER DATA
        // ========================================================

        await SeedBahrainLocationsAsync(context);
    }


    // ============================================================
    // ROLES
    // ============================================================

    private static async Task SeedRolesAsync(
        ApplicationDbContext context)
    {
        var roles = new[]
        {
            new Role
            {
                RoleName = "SuperAdmin",
                RoleDescription = "Full system access",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },

            new Role
            {
                RoleName = "Admin",
                RoleDescription = "Administrative access",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },

            new Role
            {
                RoleName = "Moderator",
                RoleDescription = "Advertisement moderation access",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },

            new Role
            {
                RoleName = "User",
                RoleDescription = "Standard registered user",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        foreach (var role in roles)
        {
            var exists = await context.Roles
                .AnyAsync(x =>
                    x.RoleName == role.RoleName);

            if (!exists)
            {
                context.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // PERMISSIONS
    // ============================================================

    private static async Task SeedPermissionsAsync(
        ApplicationDbContext context)
    {
        var permissions = new[]
        {
            new Permission
            {
                PermissionCode = "USER_VIEW",
                PermissionName = "View Users",
                Description = "View registered users",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "USER_EDIT",
                PermissionName = "Edit Users",
                Description = "Create and edit user information",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "USER_SUSPEND",
                PermissionName = "Suspend Users",
                Description = "Suspend or reactivate users",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_CREATE",
                PermissionName = "Create Advertisement",
                Description = "Create advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_EDIT",
                PermissionName = "Edit Advertisement",
                Description = "Edit advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_DELETE",
                PermissionName = "Delete Advertisement",
                Description = "Delete advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_VIEW",
                PermissionName = "View Advertisement",
                Description = "View advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_APPROVE",
                PermissionName = "Approve Advertisement",
                Description = "Approve advertisements for publication",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_REJECT",
                PermissionName = "Reject Advertisement",
                Description = "Reject advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "ADVERTISEMENT_SUSPEND",
                PermissionName = "Suspend Advertisement",
                Description = "Suspend published advertisements",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "REPORT_VIEW",
                PermissionName = "View Reports",
                Description = "View advertisement reports",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "REPORT_REVIEW",
                PermissionName = "Review Reports",
                Description = "Review and process advertisement reports",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "AUDIT_VIEW",
                PermissionName = "View Audit Logs",
                Description = "View system audit logs",
                IsActive = true
            },

            new Permission
            {
                PermissionCode = "SYSTEM_SETTINGS",
                PermissionName = "System Settings",
                Description = "Manage system settings",
                IsActive = true
            }
        };

        foreach (var permission in permissions)
        {
            var exists = await context.Permissions
                .AnyAsync(x =>
                    x.PermissionCode == permission.PermissionCode);

            if (!exists)
            {
                context.Permissions.Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // ROLE PERMISSIONS
    // ============================================================

    private static async Task SeedRolePermissionsAsync(
        ApplicationDbContext context)
    {
        var roles = await context.Roles
            .ToDictionaryAsync(
                x => x.RoleName,
                x => x.RoleID);

        var permissions = await context.Permissions
            .ToDictionaryAsync(
                x => x.PermissionCode,
                x => x.PermissionID);

        if (!roles.TryGetValue(
                "SuperAdmin",
                out var superAdminRoleId) ||
            !roles.TryGetValue(
                "Admin",
                out var adminRoleId) ||
            !roles.TryGetValue(
                "Moderator",
                out var moderatorRoleId) ||
            !roles.TryGetValue(
                "User",
                out var userRoleId))
        {
            throw new InvalidOperationException(
                "Required roles were not found in the database.");
        }

        var rolePermissionMap =
            new Dictionary<string, string[]>
            {
                ["SuperAdmin"] =
                    permissions.Keys.ToArray(),

                ["Admin"] =
                [
                    "USER_VIEW",
                    "USER_EDIT",
                    "USER_SUSPEND",

                    "ADVERTISEMENT_VIEW",
                    "ADVERTISEMENT_APPROVE",
                    "ADVERTISEMENT_REJECT",
                    "ADVERTISEMENT_SUSPEND",

                    "REPORT_VIEW",
                    "REPORT_REVIEW",

                    "AUDIT_VIEW",
                    "SYSTEM_SETTINGS"
                ],

                ["Moderator"] =
                [
                    "ADVERTISEMENT_VIEW",
                    "ADVERTISEMENT_APPROVE",
                    "ADVERTISEMENT_REJECT",
                    "ADVERTISEMENT_SUSPEND",

                    "REPORT_VIEW",
                    "REPORT_REVIEW"
                ],

                ["User"] =
                [
                    "ADVERTISEMENT_CREATE",
                    "ADVERTISEMENT_EDIT",
                    "ADVERTISEMENT_DELETE",
                    "ADVERTISEMENT_VIEW"
                ]
            };

        var roleIds =
            new Dictionary<string, int>
            {
                ["SuperAdmin"] = superAdminRoleId,
                ["Admin"] = adminRoleId,
                ["Moderator"] = moderatorRoleId,
                ["User"] = userRoleId
            };

        foreach (var roleEntry in rolePermissionMap)
        {
            var roleName = roleEntry.Key;

            if (!roleIds.TryGetValue(
                    roleName,
                    out var roleId))
            {
                continue;
            }

            foreach (var permissionCode in
                     roleEntry.Value.Distinct())
            {
                if (!permissions.TryGetValue(
                        permissionCode,
                        out var permissionId))
                {
                    continue;
                }

                var exists =
                    await context.RolePermissions
                        .AnyAsync(x =>
                            x.RoleID == roleId &&
                            x.PermissionID == permissionId);

                if (!exists)
                {
                    context.RolePermissions.Add(
                        new RolePermission
                        {
                            RoleID = roleId,
                            PermissionID = permissionId,
                            CreatedDate = DateTime.UtcNow
                        });
                }
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // ADMIN USER
    // ============================================================

    private static async Task SeedAdminUserAsync(
        ApplicationDbContext context)
    {
        // --------------------------------------------------------
        // Default development administrator details
        // --------------------------------------------------------

        const string adminUserName = "admin";
        const string adminEmail = "admin@siteads.local";
        const string adminMobileNo = "39000000";

        // Development password.
        //
        // This password is used only when:
        //
        // 1. The administrator account does not exist, OR
        // 2. The existing account still contains the old
        //    development placeholder hash.
        //
        // It will NOT overwrite an existing real password.
        //
        const string adminPassword = "Admin@12345";


        // --------------------------------------------------------
        // Find Admin role
        // --------------------------------------------------------

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(x =>
                x.RoleName == "Admin" &&
                x.IsActive);

        if (adminRole == null)
        {
            throw new InvalidOperationException(
                "Admin role was not found. " +
                "SeedRolesAsync must run before " +
                "SeedAdminUserAsync.");
        }


        // --------------------------------------------------------
        // Find existing administrator
        // --------------------------------------------------------

        var adminUser = await context.Users
            .FirstOrDefaultAsync(x =>
                x.Email == adminEmail);


        // --------------------------------------------------------
        // Create administrator if it doesn't exist
        // --------------------------------------------------------

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminUserName,
                Email = adminEmail,
                MobileNo = adminMobileNo,

                IsEmailVerified = true,
                IsMobileVerified = true,
                IsMFAEnabled = false,

                AccountStatus = "Active",

                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };


            // ----------------------------------------------------
            // Generate REAL ASP.NET Core Identity password hash
            // ----------------------------------------------------

            var passwordService =
                new PasswordService();

            adminUser.PasswordHash =
                passwordService.HashPassword(
                    adminUser,
                    adminPassword);

            context.Users.Add(adminUser);

            await context.SaveChangesAsync();
        }
        else
        {
            // ----------------------------------------------------
            // Existing administrator
            // ----------------------------------------------------

            adminUser.AccountStatus = "Active";

            adminUser.ModifiedDate =
                DateTime.UtcNow;


            // ----------------------------------------------------
            // Replace ONLY the old development placeholder hash.
            //
            // Do not reset an existing real password.
            // ----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    adminUser.PasswordHash) ||
                adminUser.PasswordHash ==
                    "DEV_HASH_REPLACE_WITH_ASPNET_IDENTITY_HASH")
            {
                var passwordService =
                    new PasswordService();

                adminUser.PasswordHash =
                    passwordService.HashPassword(
                        adminUser,
                        adminPassword);
            }

            await context.SaveChangesAsync();
        }


        // --------------------------------------------------------
        // Make sure UserProfile exists
        // --------------------------------------------------------

        var profileExists =
            await context.UserProfiles
                .AnyAsync(x =>
                    x.UserID == adminUser.UserID);

        if (!profileExists)
        {
            var profile = new UserProfile
            {
                UserID = adminUser.UserID,

                FirstName = "System",
                LastName = "Administrator",

                PreferredLanguage = "en",
                IsBusinessAccount = false,

                CreatedDate = DateTime.UtcNow
            };

            context.UserProfiles.Add(profile);

            await context.SaveChangesAsync();
        }


        // --------------------------------------------------------
        // Make sure Admin role is assigned
        // --------------------------------------------------------

        var adminRoleMappingExists =
            await context.UserRoles
                .AnyAsync(x =>
                    x.UserID == adminUser.UserID &&
                    x.RoleID == adminRole.RoleID);

        if (!adminRoleMappingExists)
        {
            context.UserRoles.Add(
                new UserRole
                {
                    UserID = adminUser.UserID,
                    RoleID = adminRole.RoleID,
                    CreatedDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();
        }
    }


    // ============================================================
    // ADVERTISEMENT STATUSES
    // ============================================================

    private static async Task SeedAdvertisementStatusesAsync(
        ApplicationDbContext context)
    {
        var statuses = new[]
        {
            new AdvertisementStatus
            {
                StatusCode = "DRAFT",
                StatusName = "Draft",
                StatusNameAr = "مسودة",
                Description =
                    "Advertisement is being prepared and has not been submitted for review.",
                DisplayOrder = 1,
                IsActive = true,
                IsPublicStatus = false
            },

            new AdvertisementStatus
            {
                StatusCode = "PENDING_REVIEW",
                StatusName = "Pending Review",
                StatusNameAr = "قيد المراجعة",
                Description =
                    "Advertisement has been submitted and is awaiting administrator review.",
                DisplayOrder = 2,
                IsActive = true,
                IsPublicStatus = false
            },

            new AdvertisementStatus
            {
                StatusCode = "REJECTED",
                StatusName = "Rejected",
                StatusNameAr = "مرفوض",
                Description =
                    "Advertisement has been rejected by an administrator.",
                DisplayOrder = 3,
                IsActive = true,
                IsPublicStatus = false
            },

            new AdvertisementStatus
            {
                StatusCode = "PUBLISHED",
                StatusName = "Published",
                StatusNameAr = "منشور",
                Description =
                    "Advertisement is approved and publicly available.",
                DisplayOrder = 4,
                IsActive = true,
                IsPublicStatus = true
            },

            new AdvertisementStatus
            {
                StatusCode = "SUSPENDED",
                StatusName = "Suspended",
                StatusNameAr = "موقوف",
                Description =
                    "Advertisement has been temporarily suspended.",
                DisplayOrder = 5,
                IsActive = true,
                IsPublicStatus = false
            },

            new AdvertisementStatus
            {
                StatusCode = "EXPIRED",
                StatusName = "Expired",
                StatusNameAr = "منتهي",
                Description =
                    "Advertisement has passed its expiry date.",
                DisplayOrder = 6,
                IsActive = true,
                IsPublicStatus = false
            },

            new AdvertisementStatus
            {
                StatusCode = "SOLD",
                StatusName = "Sold",
                StatusNameAr = "تم البيع",
                Description =
                    "Advertisement item has been sold.",
                DisplayOrder = 7,
                IsActive = true,
                IsPublicStatus = true
            },

            new AdvertisementStatus
            {
                StatusCode = "RENTED",
                StatusName = "Rented",
                StatusNameAr = "تم التأجير",
                Description =
                    "Advertisement item has been rented.",
                DisplayOrder = 8,
                IsActive = true,
                IsPublicStatus = true
            }
        };

        foreach (var status in statuses)
        {
            var exists =
                await context.AdvertisementStatuses
                    .AnyAsync(x =>
                        x.StatusCode == status.StatusCode);

            if (!exists)
            {
                context.AdvertisementStatuses.Add(status);
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // ADVERTISEMENT CATEGORIES
    // ============================================================

    private static async Task SeedAdvertisementCategoriesAsync(
        ApplicationDbContext context)
    {
        var categories = new[]
        {
            new AdvertisementCategory
            {
                CategoryName = "Property",
                Description =
                    "Properties and real estate",
                DisplayOrder = 1,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Vehicles",
                Description =
                    "Cars, motorcycles and other vehicles",
                DisplayOrder = 2,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Jobs",
                Description =
                    "Jobs and employment opportunities",
                DisplayOrder = 3,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Services",
                Description =
                    "Professional and personal services",
                DisplayOrder = 4,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Electronics",
                Description =
                    "Electronic devices and equipment",
                DisplayOrder = 5,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Furniture",
                Description =
                    "Furniture and home items",
                DisplayOrder = 6,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Business",
                Description =
                    "Businesses and commercial opportunities",
                DisplayOrder = 7,
                IsActive = true
            },

            new AdvertisementCategory
            {
                CategoryName = "Others",
                Description =
                    "Other advertisements",
                DisplayOrder = 8,
                IsActive = true
            }
        };

        foreach (var category in categories)
        {
            var exists =
                await context.AdvertisementCategories
                    .AnyAsync(x =>
                        x.CategoryName ==
                        category.CategoryName);

            if (!exists)
            {
                context.AdvertisementCategories
                    .Add(category);
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // ADVERTISEMENT TYPES
    // ============================================================

    private static async Task SeedAdvertisementTypesAsync(
        ApplicationDbContext context)
    {
        var types = new[]
        {
            new AdvertisementType
            {
                TypeName = "For Sale",
                Description =
                    "Items or properties available for sale",
                IsActive = true
            },

            new AdvertisementType
            {
                TypeName = "For Rent",
                Description =
                    "Items or properties available for rent",
                IsActive = true
            },

            new AdvertisementType
            {
                TypeName = "Wanted",
                Description =
                    "User is looking for an item, property or service",
                IsActive = true
            },

            new AdvertisementType
            {
                TypeName = "Offer",
                Description =
                    "Offer or opportunity",
                IsActive = true
            },

            new AdvertisementType
            {
                TypeName = "Service",
                Description =
                    "Service advertisement",
                IsActive = true
            }
        };

        foreach (var type in types)
        {
            var exists =
                await context.AdvertisementTypes
                    .AnyAsync(x =>
                        x.TypeName == type.TypeName);

            if (!exists)
            {
                context.AdvertisementTypes.Add(type);
            }
        }

        await context.SaveChangesAsync();
    }


    // ============================================================
    // BAHRAIN LOCATIONS
    // ============================================================

    private static async Task SeedBahrainLocationsAsync(
        ApplicationDbContext context)
    {
        // --------------------------------------------------------
        // Country
        // --------------------------------------------------------

        var bahrain =
            await context.Countries
                .FirstOrDefaultAsync(x =>
                    x.CountryCode == "BH");

        if (bahrain == null)
        {
            bahrain = new Country
            {
                CountryCode = "BH",
                CountryName = "Bahrain",
                IsActive = true
            };

            context.Countries.Add(bahrain);

            await context.SaveChangesAsync();
        }


        // --------------------------------------------------------
        // Governorates
        // --------------------------------------------------------

        var governorateNames = new[]
        {
            "Capital Governorate",
            "Muharraq Governorate",
            "Northern Governorate",
            "Southern Governorate"
        };

        foreach (var governorateName in governorateNames)
        {
            var exists =
                await context.States
                    .AnyAsync(x =>
                        x.CountryID == bahrain.CountryID &&
                        x.StateName == governorateName);

            if (!exists)
            {
                context.States.Add(
                    new State
                    {
                        CountryID = bahrain.CountryID,
                        StateName = governorateName,
                        IsActive = true
                    });
            }
        }

        await context.SaveChangesAsync();
    }
}