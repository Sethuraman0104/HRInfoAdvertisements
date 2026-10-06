using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdminProfileService : IAdminProfileService
{
    private readonly ApplicationDbContext _context;

    public AdminProfileService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET ADMIN PROFILE
    // ============================================================

    public async Task<AdminProfileResponse?> GetProfileAsync(
        long adminUserId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.UserProfile)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserID == adminUserId);

        if (user == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Primary address
        // --------------------------------------------------------

        var primaryAddress = await _context.UserAddresses
            .AsNoTracking()
            .Where(x =>
                x.UserID == adminUserId &&
                x.IsPrimary)
            .Select(x => new AdminAddressDto
            {
                AddressType = "Primary",

                CountryName = x.Country != null
                    ? x.Country.CountryName
                    : null,

                StateName = x.State != null
                    ? x.State.StateName
                    : null,

                CityName = x.City != null
                    ? x.City.CityName
                    : null,

                AreaName = x.Area != null
                    ? x.Area.AreaName
                    : null,

                PostalCode = x.PostalCode,
                BuildingNo = x.BuildingNo,
                RoadNo = x.RoadNo,
                BlockNo = x.BlockNo,
                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2
            })
            .FirstOrDefaultAsync();

        // --------------------------------------------------------
        // Roles
        // --------------------------------------------------------

        var roles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => new AdminRoleDto
            {
                RoleID = x.Role.RoleID,
                RoleName = x.Role.RoleName,
                RoleDescription = x.Role.RoleDescription,
                IsActive = x.Role.IsActive
            })
            .OrderBy(x => x.RoleName)
            .ToList();

        // --------------------------------------------------------
        // Active sessions
        // --------------------------------------------------------

        var now = DateTime.UtcNow;

        var activeSessions = await _context.UserSessions
            .AsNoTracking()
            .Where(x =>
                x.UserID == adminUserId &&
                x.RevokedDate == null &&
                (
                    !x.ExpiresAt.HasValue ||
                    x.ExpiresAt.Value > now
                ))
            .OrderByDescending(x => x.LastActivityDate)
            .Select(x => new AdminSessionDto
            {
                UserSessionID = x.UserSessionID,
                DeviceType = x.DeviceType,
                DeviceName = x.DeviceName,
                IPAddress = x.IPAddress,
                LastActivityDate = x.LastActivityDate,

                // The current JWT is not directly linked to
                // UserSession.SessionTokenHash in this service.
                IsCurrentSession = false
            })
            .ToListAsync();

        // --------------------------------------------------------
        // Profile
        // --------------------------------------------------------

        AdminProfileDto? profile = null;

        if (user.UserProfile != null)
        {
            profile = new AdminProfileDto
            {
                FirstName = user.UserProfile.FirstName,
                LastName = user.UserProfile.LastName,
                Nationality = user.UserProfile.Nationality,
                PreferredLanguage =
                    user.UserProfile.PreferredLanguage,
                ProfilePhotoURL =
                    user.UserProfile.ProfilePhotoURL
            };
        }

        // --------------------------------------------------------
        // Final response
        // --------------------------------------------------------

        return new AdminProfileResponse
        {
            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,
            MobileNo = user.MobileNo,

            IsActive =
                string.Equals(
                    user.AccountStatus,
                    "Active",
                    StringComparison.OrdinalIgnoreCase),

            IsEmailVerified = user.IsEmailVerified,
            IsMobileVerified = user.IsMobileVerified,
            IsMFAEnabled = user.IsMFAEnabled,

            FailedLoginAttempts =
                user.FailedLoginAttempts,

            LastLoginDate = user.LastLoginDate,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate,
            LockoutEndDate = user.LockoutEndDate,

            AccountStatus = user.AccountStatus,

            Profile = profile,

            PrimaryAddress = primaryAddress,

            Roles = roles,

            ActiveSessions = activeSessions
        };
    }

    // ============================================================
    // UPDATE ADMIN PROFILE
    // ============================================================

    public async Task<AdminProfileResponse?> UpdateProfileAsync(
        long adminUserId,
        UpdateAdminProfileRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException(
                "Profile update request is required.");
        }

        var firstName = request.FirstName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (firstName.Length > 100)
        {
            throw new ArgumentException(
                "First name cannot exceed 100 characters.");
        }

        var lastName = request.LastName?.Trim();

        if (lastName?.Length > 100)
        {
            throw new ArgumentException(
                "Last name cannot exceed 100 characters.");
        }

        var nationality = request.Nationality?.Trim();

        if (nationality?.Length > 100)
        {
            throw new ArgumentException(
                "Nationality cannot exceed 100 characters.");
        }

        var preferredLanguage =
            request.PreferredLanguage?.Trim();

        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            preferredLanguage = "en";
        }

        if (preferredLanguage.Length > 10)
        {
            throw new ArgumentException(
                "Preferred language cannot exceed 10 characters.");
        }

        var profilePhotoUrl =
            request.ProfilePhotoURL?.Trim();

        if (profilePhotoUrl?.Length > 1000)
        {
            throw new ArgumentException(
                "Profile photo URL cannot exceed 1000 characters.");
        }

        // --------------------------------------------------------
        // Make sure the administrator exists
        // --------------------------------------------------------

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserID == adminUserId);

        if (user == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Get existing profile
        // --------------------------------------------------------

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x =>
                x.UserID == adminUserId);

        if (profile == null)
        {
            profile = new Domain.Entities.UserProfile
            {
                UserID = adminUserId,

                FirstName = firstName,
                LastName = lastName,
                Nationality = nationality,
                PreferredLanguage = preferredLanguage,
                ProfilePhotoURL = profilePhotoUrl,

                IsBusinessAccount = false,
                CompanyName = null,

                CreatedDate = DateTime.UtcNow
            };

            _context.UserProfiles.Add(profile);
        }
        else
        {
            profile.FirstName = firstName;
            profile.LastName = lastName;
            profile.Nationality = nationality;
            profile.PreferredLanguage = preferredLanguage;
            profile.ProfilePhotoURL = profilePhotoUrl;

            profile.ModifiedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Return the complete updated administrator profile.
        return await GetProfileAsync(adminUserId);
    }

    // ============================================================
    // REVOKE ADMIN SESSION
    // ============================================================

    public async Task<bool> RevokeSessionAsync(
        long adminUserId,
        long sessionId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(x =>
                x.UserSessionID == sessionId &&
                x.UserID == adminUserId &&
                x.RevokedDate == null);

        if (session == null)
        {
            return false;
        }

        session.RevokedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}