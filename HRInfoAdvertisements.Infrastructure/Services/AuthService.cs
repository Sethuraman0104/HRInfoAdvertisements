using HRInfoAdvertisements.Application.DTOs.Authentication;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Settings;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ApplicationDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _jwtSettings = jwtOptions.Value;
    }

    // ============================================================
    // REGISTER
    // ============================================================

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        // --------------------------------------------------------
        // Normalize input
        // --------------------------------------------------------

        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var mobileNo = request.MobileNo.Trim();

        // --------------------------------------------------------
        // Check duplicate username
        // --------------------------------------------------------

        var usernameExists = await _context.Users
            .AnyAsync(x => x.UserName == userName);

        if (usernameExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username already exists."
            };
        }

        // --------------------------------------------------------
        // Check duplicate email
        // --------------------------------------------------------

        var emailExists = await _context.Users
            .AnyAsync(x => x.Email.ToLower() == email);

        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email address already exists."
            };
        }

        // --------------------------------------------------------
        // Check duplicate mobile
        // --------------------------------------------------------

        var mobileExists = await _context.Users
            .AnyAsync(x => x.MobileNo == mobileNo);

        if (mobileExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mobile number already exists."
            };
        }

        // --------------------------------------------------------
        // Begin transaction
        // --------------------------------------------------------

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // ----------------------------------------------------
            // Create User
            // ----------------------------------------------------

            var user = new User
            {
                UserName = userName,
                Email = email,
                MobileNo = mobileNo,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsMFAEnabled = false,

                AccountStatus = "Active",

                CreatedDate = DateTime.UtcNow
            };

            // ----------------------------------------------------
            // Hash Password
            // ----------------------------------------------------

            user.PasswordHash =
                _passwordService.HashPassword(
                    user,
                    request.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // ----------------------------------------------------
            // Create User Profile
            // ----------------------------------------------------

            var profile = new UserProfile
            {
                UserID = user.UserID,

                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),

                PreferredLanguage = "en",
                IsBusinessAccount = false,

                CreatedDate = DateTime.UtcNow
            };

            _context.UserProfiles.Add(profile);

            // ----------------------------------------------------
            // Assign Default User Role
            // ----------------------------------------------------

            var userRole = await _context.Roles
                .FirstOrDefaultAsync(x =>
                    x.RoleName == "User" &&
                    x.IsActive);

            if (userRole == null)
            {
                throw new InvalidOperationException(
                    "Default User role was not found.");
            }

            var userRoleMapping = new UserRole
            {
                UserID = user.UserID,
                RoleID = userRole.RoleID,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRoleMapping);

            await _context.SaveChangesAsync();

            // ----------------------------------------------------
            // Load Permissions
            // ----------------------------------------------------

            var permissions = await _context.RolePermissions
                .Where(x =>
                    x.RoleID == userRole.RoleID &&
                    x.Permission.IsActive)
                .Select(x => x.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

            var roles = new List<string>
            {
                userRole.RoleName
            };

            // ----------------------------------------------------
            // Generate Access Token
            // ----------------------------------------------------

            var accessToken =
                _jwtService.GenerateAccessToken(
                    user,
                    roles,
                    permissions);

            // ----------------------------------------------------
            // Generate Refresh Token
            // ----------------------------------------------------

            var refreshToken =
                TokenHelper.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserID = user.UserID,

                TokenHash =
                    TokenHelper.HashToken(
                        refreshToken),

                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        _jwtSettings.RefreshTokenDays),

                CreatedDate = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(
                refreshTokenEntity);

            await _context.SaveChangesAsync();

            // ----------------------------------------------------
            // Commit
            // ----------------------------------------------------

            await transaction.CommitAsync();

            // ----------------------------------------------------
            // Return Response
            // ----------------------------------------------------

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful.",

                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,

                AccessToken = accessToken,
                RefreshToken = refreshToken,

                ExpiresAt =
                    _jwtService.GetAccessTokenExpiration(),

                Roles = roles,
                Permissions = permissions
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ============================================================
    // LOGIN
    // ============================================================

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var email =
            request.Email.Trim().ToLowerInvariant();

        // --------------------------------------------------------
        // Find User
        // --------------------------------------------------------

        var user = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // --------------------------------------------------------
        // Check Account Status
        // --------------------------------------------------------

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User account is not active."
            };
        }

        // --------------------------------------------------------
        // Verify Password
        // --------------------------------------------------------

        var passwordValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (!passwordValid)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var activeRoles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.Role.RoleName)
            .Distinct()
            .ToList();

        var roleIds = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.RoleID)
            .Distinct()
            .ToList();

        // --------------------------------------------------------
        // Load Permissions
        // --------------------------------------------------------

        var permissions =
            await _context.RolePermissions
                .Where(x =>
                    roleIds.Contains(x.RoleID) &&
                    x.Permission.IsActive)
                .Select(x => x.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

        // --------------------------------------------------------
        // Generate Access Token
        // --------------------------------------------------------

        var accessToken =
            _jwtService.GenerateAccessToken(
                user,
                activeRoles,
                permissions);

        // --------------------------------------------------------
        // Generate Refresh Token
        // --------------------------------------------------------

        var refreshToken =
            TokenHelper.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserID = user.UserID,

            TokenHash =
                TokenHelper.HashToken(
                    refreshToken),

            ExpiresAt =
                DateTime.UtcNow.AddDays(
                    _jwtSettings.RefreshTokenDays),

            CreatedDate = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(
            refreshTokenEntity);

        // --------------------------------------------------------
        // Update Login Information
        // --------------------------------------------------------

        user.LastLoginDate = DateTime.UtcNow;
        user.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // Return Response
        // --------------------------------------------------------

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",

            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,

            AccessToken = accessToken,
            RefreshToken = refreshToken,

            ExpiresAt =
                _jwtService.GetAccessTokenExpiration(),

            Roles = activeRoles,
            Permissions = permissions
        };
    }

    // ============================================================
    // REFRESH TOKEN
    // ============================================================

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request)
    {
        // --------------------------------------------------------
        // Validate Request
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                request.RefreshToken))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Refresh token is required."
            };
        }

        var rawRefreshToken =
            request.RefreshToken.Trim();

        // --------------------------------------------------------
        // Hash Supplied Token
        // --------------------------------------------------------

        var tokenHash =
            TokenHelper.HashToken(
                rawRefreshToken);

        // --------------------------------------------------------
        // Find Existing Refresh Token
        // --------------------------------------------------------

        var existingToken =
            await _context.RefreshTokens
                .Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                        .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash);

        if (existingToken == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid refresh token."
            };
        }

        // --------------------------------------------------------
        // Check Revoked
        // --------------------------------------------------------

        if (existingToken.RevokedAt.HasValue)
        {
            return new AuthResponse
            {
                Success = false,
                Message =
                    "Refresh token has already been revoked."
            };
        }

        // --------------------------------------------------------
        // Check Expiration
        // --------------------------------------------------------

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Refresh token has expired."
            };
        }

        // --------------------------------------------------------
        // Get User
        // --------------------------------------------------------

        var user = existingToken.User;

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message =
                    "User associated with refresh token was not found."
            };
        }

        // --------------------------------------------------------
        // Check Account Status
        // --------------------------------------------------------

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User account is not active."
            };
        }

        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var activeRoles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.Role.RoleName)
            .Distinct()
            .ToList();

        var roleIds = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.RoleID)
            .Distinct()
            .ToList();

        // --------------------------------------------------------
        // Load Permissions
        // --------------------------------------------------------

        var permissions =
            await _context.RolePermissions
                .Where(x =>
                    roleIds.Contains(x.RoleID) &&
                    x.Permission.IsActive)
                .Select(x =>
                    x.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

        // --------------------------------------------------------
        // Generate New Access Token
        // --------------------------------------------------------

        var accessToken =
            _jwtService.GenerateAccessToken(
                user,
                activeRoles,
                permissions);

        // --------------------------------------------------------
        // Generate New Refresh Token
        // --------------------------------------------------------

        var newRefreshToken =
            TokenHelper.GenerateRefreshToken();

        var newRefreshTokenHash =
            TokenHelper.HashToken(
                newRefreshToken);

        var newRefreshTokenEntity =
            new RefreshToken
            {
                UserID = user.UserID,

                TokenHash =
                    newRefreshTokenHash,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        _jwtSettings.RefreshTokenDays),

                CreatedDate = DateTime.UtcNow
            };

        // --------------------------------------------------------
        // Revoke Old Refresh Token
        // --------------------------------------------------------

        existingToken.RevokedAt =
            DateTime.UtcNow;

        existingToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        _context.RefreshTokens.Add(
            newRefreshTokenEntity);

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // Return New Tokens
        // --------------------------------------------------------

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",

            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,

            AccessToken = accessToken,
            RefreshToken = newRefreshToken,

            ExpiresAt =
                _jwtService.GetAccessTokenExpiration(),

            Roles = activeRoles,
            Permissions = permissions
        };
    }

    // ============================================================
    // LOGOUT
    // ============================================================

    public async Task<bool> LogoutAsync(
        long userId,
        string refreshToken)
    {
        // --------------------------------------------------------
        // Validate Refresh Token
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var tokenHash =
            TokenHelper.HashToken(
                refreshToken.Trim());

        // --------------------------------------------------------
        // Find User's Refresh Token
        // --------------------------------------------------------

        var existingToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.TokenHash == tokenHash);

        if (existingToken == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Check Already Revoked
        // --------------------------------------------------------

        if (existingToken.RevokedAt.HasValue)
        {
            return false;
        }

        // --------------------------------------------------------
        // Revoke Token
        // --------------------------------------------------------

        existingToken.RevokedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // FORGOT PASSWORD
    // ============================================================

    public async Task<bool> ForgotPasswordAsync(
        ForgotPasswordRequest request)
    {
        // --------------------------------------------------------
        // Validate Request
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return false;
        }

        var email =
            request.Email.Trim().ToLowerInvariant();

        // --------------------------------------------------------
        // Find User
        // --------------------------------------------------------

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        // --------------------------------------------------------
        // Do not reveal whether email exists.
        // --------------------------------------------------------

        if (user == null)
        {
            return true;
        }

        // --------------------------------------------------------
        // Check Account Status
        // --------------------------------------------------------

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // --------------------------------------------------------
        // Invalidate Previous Password Reset OTPs
        // --------------------------------------------------------

        var existingOtps =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == "ForgotPassword" &&
                    !x.IsConsumed)
                .ToListAsync();

        foreach (var existingOtp in existingOtps)
        {
            existingOtp.IsConsumed = true;
            existingOtp.ConsumedDate = DateTime.UtcNow;
        }

        // --------------------------------------------------------
        // Generate 6-Digit OTP
        // --------------------------------------------------------

        var otp =
            Random.Shared
                .Next(100000, 1000000)
                .ToString();

        // --------------------------------------------------------
        // Hash OTP
        // --------------------------------------------------------

        var otpHash =
            TokenHelper.HashToken(otp);

        // --------------------------------------------------------
        // Create OTP Request
        // --------------------------------------------------------

        var otpRequest = new OTPRequest
        {
            UserID = user.UserID,

            Destination = user.Email,

            Purpose = "ForgotPassword",

            OTPHash = otpHash,

            ExpiresAt =
                DateTime.UtcNow.AddMinutes(10),

            AttemptCount = 0,

            MaxAttempts = 5,

            IsConsumed = false,

            ConsumedDate = null,

            CreatedDate = DateTime.UtcNow
        };

        _context.OTPRequests.Add(
            otpRequest);

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // DEVELOPMENT ONLY
        //
        // Production:
        // Send OTP through Email/SMS service.
        //
        // Never log OTP in production.
        // --------------------------------------------------------

        Console.WriteLine(
            $"[DEV OTP] Password reset OTP for {user.Email}: {otp}");

        return true;
    }

    // ============================================================
    // RESET PASSWORD
    // ============================================================

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request)
    {
        // --------------------------------------------------------
        // Validate Request
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.OTP) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return false;
        }

        var email =
            request.Email.Trim().ToLowerInvariant();

        var otp =
            request.OTP.Trim();

        // --------------------------------------------------------
        // Find User
        // --------------------------------------------------------

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        if (user == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Check Account Status
        // --------------------------------------------------------

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // --------------------------------------------------------
        // Hash Supplied OTP
        // --------------------------------------------------------

        var otpHash =
            TokenHelper.HashToken(otp);

        // --------------------------------------------------------
        // Find Latest Matching OTP
        // --------------------------------------------------------

        var otpRequest =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == "ForgotPassword" &&
                    x.OTPHash == otpHash &&
                    !x.IsConsumed)
                .OrderByDescending(x =>
                    x.CreatedDate)
                .FirstOrDefaultAsync();

        // --------------------------------------------------------
        // Invalid OTP
        // --------------------------------------------------------

        if (otpRequest == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Check Maximum Attempts
        // --------------------------------------------------------

        if (otpRequest.AttemptCount >=
            otpRequest.MaxAttempts)
        {
            otpRequest.IsConsumed = true;
            otpRequest.ConsumedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return false;
        }

        // --------------------------------------------------------
        // Check OTP Expiration
        // --------------------------------------------------------

        if (otpRequest.ExpiresAt <= DateTime.UtcNow)
        {
            otpRequest.IsConsumed = true;
            otpRequest.ConsumedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return false;
        }

        // --------------------------------------------------------
        // Mark OTP as Consumed
        // --------------------------------------------------------

        otpRequest.IsConsumed = true;
        otpRequest.ConsumedDate = DateTime.UtcNow;

        // --------------------------------------------------------
        // Hash New Password
        // --------------------------------------------------------

        user.PasswordHash =
            _passwordService.HashPassword(
                user,
                request.NewPassword);

        user.ModifiedDate =
            DateTime.UtcNow;

        // --------------------------------------------------------
        // Revoke Existing Refresh Tokens
        //
        // All existing refresh tokens are invalidated so that
        // existing sessions cannot continue after password reset.
        // --------------------------------------------------------

        var activeRefreshTokens =
            await _context.RefreshTokens
                .Where(x =>
                    x.UserID == user.UserID &&
                    !x.RevokedAt.HasValue &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

        foreach (var token in activeRefreshTokens)
        {
            token.RevokedAt =
                DateTime.UtcNow;
        }

        // --------------------------------------------------------
        // Save Changes
        // --------------------------------------------------------

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // CURRENT USER
    // ============================================================

    public async Task<AuthResponse> GetCurrentUserAsync(
        long userId)
    {
        // --------------------------------------------------------
        // Find User
        // --------------------------------------------------------

        var user = await _context.Users
            .Include(x => x.UserProfile)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var roles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.Role.RoleName)
            .Distinct()
            .ToList();

        var roleIds = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.RoleID)
            .Distinct()
            .ToList();

        // --------------------------------------------------------
        // Load Permissions
        // --------------------------------------------------------

        var permissions =
            await _context.RolePermissions
                .Where(x =>
                    roleIds.Contains(x.RoleID) &&
                    x.Permission.IsActive)
                .Select(x =>
                    x.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

        // --------------------------------------------------------
        // Return Current User
        // --------------------------------------------------------

        return new AuthResponse
        {
            Success = true,

            Message =
                "User information retrieved successfully.",

            UserID = user.UserID,

            UserName = user.UserName,

            Email = user.Email,

            Roles = roles,

            Permissions = permissions
        };
    }
}