using HRInfoAdvertisements.Application.DTOs.UserManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly ApplicationDbContext _context;

    public UserManagementService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET USERS
    // ============================================================

    public async Task<UserListResponse> GetUsersAsync(
        UserListRequest request)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize < 1
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _context.Users
            .AsNoTracking()
            .Include(x => x.UserProfile)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .AsQueryable();

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.UserName.Contains(search) ||
                x.Email.Contains(search) ||
                (x.MobileNo != null &&
                 x.MobileNo.Contains(search)) ||
                (x.UserProfile != null &&
                 x.UserProfile.FirstName.Contains(search)) ||
                (x.UserProfile != null &&
                 x.UserProfile.LastName != null &&
                 x.UserProfile.LastName.Contains(search)));
        }

        // --------------------------------------------------------
        // Account Status Filter
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.AccountStatus))
        {
            var accountStatus =
                request.AccountStatus.Trim();

            query = query.Where(x =>
                x.AccountStatus == accountStatus);
        }

        // --------------------------------------------------------
        // Role Filter
        // --------------------------------------------------------

        if (request.RoleID.HasValue)
        {
            var roleId = request.RoleID.Value;

            query = query.Where(x =>
                x.UserRoles.Any(r =>
                    r.RoleID == roleId));
        }

        // --------------------------------------------------------
        // Total Records
        // --------------------------------------------------------

        var totalRecords =
            await query.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords / (double)pageSize);

        // --------------------------------------------------------
        // Get Page
        // --------------------------------------------------------

        var users = await query
            .OrderByDescending(x => x.CreatedDate)
            .ThenBy(x => x.UserID)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // --------------------------------------------------------
        // Map Response
        // --------------------------------------------------------

        var items = users
            .Select(x => new UserListItemResponse
            {
                UserID = x.UserID,
                UserName = x.UserName,
                Email = x.Email,
                MobileNo = x.MobileNo,

                FirstName =
                    x.UserProfile?.FirstName,

                LastName =
                    x.UserProfile?.LastName,

                AccountStatus =
                    x.AccountStatus,

                IsEmailVerified =
                    x.IsEmailVerified,

                IsMobileVerified =
                    x.IsMobileVerified,

                IsMFAEnabled =
                    x.IsMFAEnabled,

                FailedLoginAttempts =
                    x.FailedLoginAttempts,

                LockoutEndDate =
                    x.LockoutEndDate,

                LastLoginDate =
                    x.LastLoginDate,

                CreatedDate =
                    x.CreatedDate,

                Roles = x.UserRoles
                    .Where(r =>
                        r.Role != null &&
                        r.Role.IsActive)
                    .Select(r => r.Role.RoleName)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList()
            })
            .ToList();

        return new UserListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // ============================================================
    // GET USER BY ID
    // ============================================================

    public async Task<UserDetailResponse?> GetUserByIdAsync(
        long userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.UserProfile)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        if (user == null)
        {
            return null;
        }

        return new UserDetailResponse
        {
            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,
            MobileNo = user.MobileNo,

            AccountStatus =
                user.AccountStatus,

            IsEmailVerified =
                user.IsEmailVerified,

            IsMobileVerified =
                user.IsMobileVerified,

            IsMFAEnabled =
                user.IsMFAEnabled,

            FailedLoginAttempts =
                user.FailedLoginAttempts,

            LockoutEndDate =
                user.LockoutEndDate,

            LastLoginDate =
                user.LastLoginDate,

            CreatedDate =
                user.CreatedDate,

            ModifiedDate =
                user.ModifiedDate,

            Profile =
                user.UserProfile == null
                    ? null
                    : new UserProfileResponse
                    {
                        UserProfileID =
                            user.UserProfile.UserProfileID,

                        FirstName =
                            user.UserProfile.FirstName,

                        LastName =
                            user.UserProfile.LastName,

                        ProfilePhotoURL =
                            user.UserProfile.ProfilePhotoURL,

                        Nationality =
                            user.UserProfile.Nationality,

                        PreferredLanguage =
                            user.UserProfile.PreferredLanguage,

                        IsBusinessAccount =
                            user.UserProfile.IsBusinessAccount,

                        CompanyName =
                            user.UserProfile.CompanyName
                    },

            Roles = user.UserRoles
                .Where(x => x.Role != null)
                .Select(x => new UserRoleResponse
                {
                    RoleID =
                        x.RoleID,

                    RoleName =
                        x.Role.RoleName,

                    RoleDescription =
                        x.Role.RoleDescription,

                    IsActive =
                        x.Role.IsActive
                })
                .OrderBy(x => x.RoleName)
                .ToList()
        };
    }

    // ============================================================
    // UPDATE ACCOUNT STATUS
    // ============================================================

    public async Task<bool> UpdateUserStatusAsync(
        long userId,
        string accountStatus,
        long modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(accountStatus))
        {
            return false;
        }

        var status =
            accountStatus.Trim();

        var allowedStatuses =
            new[]
            {
                "Active",
                "Inactive",
                "Suspended"
            };

        if (!allowedStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        if (user == null)
        {
            return false;
        }

        user.AccountStatus =
            allowedStatuses.First(x =>
                string.Equals(
                    x,
                    status,
                    StringComparison.OrdinalIgnoreCase));

        user.ModifiedBy = modifiedBy;
        user.ModifiedDate = DateTime.UtcNow;

        // --------------------------------------------------------
        // If account is reactivated, clear lock information.
        // --------------------------------------------------------

        if (string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEndDate = null;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // UPDATE USER LOCK
    // ============================================================

    public async Task<bool> UpdateUserLockAsync(
        long userId,
        bool isLocked,
        DateTime? lockoutEndDate,
        long modifiedBy)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        if (user == null)
        {
            return false;
        }

        if (isLocked)
        {
            if (!lockoutEndDate.HasValue)
            {
                return false;
            }

            if (lockoutEndDate.Value <= DateTime.UtcNow)
            {
                return false;
            }

            user.LockoutEndDate =
                lockoutEndDate.Value;

            user.FailedLoginAttempts = 0;
        }
        else
        {
            user.LockoutEndDate = null;
            user.FailedLoginAttempts = 0;
        }

        user.ModifiedBy = modifiedBy;
        user.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // ASSIGN ROLE
    // ============================================================

    public async Task<bool> AssignRoleAsync(
        long userId,
        int roleId,
        long modifiedBy)
    {
        var userExists =
            await _context.Users
                .AnyAsync(x => x.UserID == userId);

        if (!userExists)
        {
            return false;
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(x =>
                x.RoleID == roleId &&
                x.IsActive);

        if (role == null)
        {
            return false;
        }

        var existingMapping =
            await _context.UserRoles
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.RoleID == roleId);

        if (existingMapping != null)
        {
            return true;
        }

        _context.UserRoles.Add(
            new UserRole
            {
                UserID = userId,
                RoleID = roleId,
                CreatedDate = DateTime.UtcNow
            });

        var user = await _context.Users
            .FirstAsync(x =>
                x.UserID == userId);

        user.ModifiedBy = modifiedBy;
        user.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // REMOVE ROLE
    // ============================================================

    public async Task<bool> RemoveRoleAsync(
        long userId,
        int roleId,
        long modifiedBy)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        if (user == null)
        {
            return false;
        }

        var userRole = await _context.UserRoles
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                x.RoleID == roleId);

        if (userRole == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Protect the last SuperAdmin
        // --------------------------------------------------------

        if (string.Equals(
                userRole.Role.RoleName,
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase))
        {
            var superAdminCount =
                await _context.UserRoles
                    .Where(x =>
                        x.RoleID == roleId)
                    .Join(
                        _context.Users,
                        ur => ur.UserID,
                        u => u.UserID,
                        (ur, u) => u)
                    .CountAsync(x =>
                        x.AccountStatus == "Active");

            if (superAdminCount <= 1)
            {
                return false;
            }
        }

        // --------------------------------------------------------
        // Protect the last active Admin/SuperAdmin account
        // --------------------------------------------------------

        if (string.Equals(
                userRole.Role.RoleName,
                "Admin",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                userRole.Role.RoleName,
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase))
        {
            var hasOtherAdministrativeUser =
                await _context.UserRoles
                    .Where(x =>
                        x.UserID != userId)
                    .Join(
                        _context.Roles,
                        ur => ur.RoleID,
                        r => r.RoleID,
                        (ur, r) => new
                        {
                            ur.UserID,
                            r.RoleName,
                            r.IsActive
                        })
                    .Join(
                        _context.Users,
                        x => x.UserID,
                        u => u.UserID,
                        (x, u) => new
                        {
                            x.RoleName,
                            x.IsActive,
                            u.AccountStatus
                        })
                    .AnyAsync(x =>
                        x.IsActive &&
                        x.AccountStatus == "Active" &&
                        (
                            x.RoleName == "Admin" ||
                            x.RoleName == "SuperAdmin"
                        ));

            if (!hasOtherAdministrativeUser)
            {
                return false;
            }
        }

        _context.UserRoles.Remove(userRole);

        user.ModifiedBy = modifiedBy;
        user.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}