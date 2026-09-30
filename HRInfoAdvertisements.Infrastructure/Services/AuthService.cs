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
    private const string EmailVerificationPurpose = "EmailVerification";
    private const string ForgotPasswordPurpose = "ForgotPassword";

    private const int OtpExpiryMinutes = 10;
    private const int OtpMaxAttempts = 5;
    private const int ResendCooldownSeconds = 60;

    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ApplicationDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _jwtSettings = jwtOptions.Value;
    }

    // ============================================================
    // REGISTER
    // ============================================================

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        if (request == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Registration request is required."
            };
        }

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

            var now = DateTime.UtcNow;

            var user = new User
            {
                UserName = userName,
                Email = email,
                MobileNo = mobileNo,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsMFAEnabled = false,

                AccountStatus = "Active",

                CreatedDate = now
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

                CreatedDate = now
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
                CreatedDate = now
            };

            _context.UserRoles.Add(userRoleMapping);

            // ----------------------------------------------------
            // Generate Email Verification OTP
            // ----------------------------------------------------

            var otp = GenerateOtp();

            var otpRequest = new OTPRequest
            {
                UserID = user.UserID,

                Destination = user.Email,

                Purpose = EmailVerificationPurpose,

                OTPHash =
                    TokenHelper.HashToken(otp),

                ExpiresAt =
                    now.AddMinutes(OtpExpiryMinutes),

                AttemptCount = 0,

                MaxAttempts = OtpMaxAttempts,

                IsConsumed = false,

                ConsumedDate = null,

                CreatedDate = now
            };

            _context.OTPRequests.Add(otpRequest);

            await _context.SaveChangesAsync();

            // ----------------------------------------------------
            // Send Verification Email
            // ----------------------------------------------------

            await SendEmailVerificationOtpAsync(
                user,
                otp);

            // ----------------------------------------------------
            // Commit
            // ----------------------------------------------------

            await transaction.CommitAsync();

            // ----------------------------------------------------
            // IMPORTANT:
            //
            // Do NOT generate JWT tokens here.
            //
            // The user must verify the email OTP first.
            // ----------------------------------------------------

            return new AuthResponse
            {
                Success = true,

                Message =
                    "Registration successful. A verification code has been sent to your email address.",

                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,

                AccessToken = string.Empty,
                RefreshToken = string.Empty,

                ExpiresAt = default,

                Roles = new List<string>
                {
                    userRole.RoleName
                },

                Permissions = new List<string>(),

                RequiresEmailVerification = true
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
        if (request == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Login request is required."
            };
        }

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
        // EMAIL VERIFICATION CHECK
        //
        // Correct password + unverified email:
        // generate a new OTP and require verification.
        // --------------------------------------------------------

        if (!user.IsEmailVerified)
        {
            var otp = await CreateEmailVerificationOtpAsync(user);

            await SendEmailVerificationOtpAsync(
                user,
                otp);

            return new AuthResponse
            {
                Success = true,

                Message =
                    "Your email address has not been verified. A verification code has been sent to your email address.",

                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,

                AccessToken = string.Empty,
                RefreshToken = string.Empty,

                ExpiresAt = default,

                RequiresEmailVerification = true,

                Roles = user.UserRoles
                    .Where(x =>
                        x.Role != null &&
                        x.Role.IsActive)
                    .Select(x => x.Role!.RoleName)
                    .Distinct()
                    .ToList(),

                Permissions = new List<string>()
            };
        }

        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var activeRoles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.Role!.RoleName)
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
            Permissions = permissions,

            RequiresEmailVerification = false
        };
    }

    // ============================================================
    // VERIFY EMAIL OTP
    // ============================================================

    public async Task<AuthResponse> VerifyEmailOtpAsync(
        VerifyEmailOtpRequest request)
    {
        // --------------------------------------------------------
        // Validate Request
        // --------------------------------------------------------

        if (request == null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.OTP))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email address and verification code are required."
            };
        }

        var email =
            request.Email.Trim().ToLowerInvariant();

        var otp =
            request.OTP.Trim();

        if (otp.Length != 6 ||
            !otp.All(char.IsDigit))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Please enter a valid 6-digit verification code."
            };
        }

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
                Message = "Invalid verification request."
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
        // Already Verified
        // --------------------------------------------------------

        if (user.IsEmailVerified)
        {
            return await CreateAuthenticatedResponseAsync(
                user,
                "Email address is already verified.");
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
                    x.Destination == user.Email &&
                    x.Purpose == EmailVerificationPurpose &&
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
            return new AuthResponse
            {
                Success = false,
                Message = "The verification code is invalid."
            };
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

            return new AuthResponse
            {
                Success = false,
                Message =
                    "This verification code has expired because the maximum number of attempts was reached."
            };
        }

        // --------------------------------------------------------
        // Check Expiration
        // --------------------------------------------------------

        if (otpRequest.ExpiresAt <= DateTime.UtcNow)
        {
            otpRequest.IsConsumed = true;
            otpRequest.ConsumedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = false,
                Message =
                    "This verification code has expired. Please request a new code."
            };
        }

        // --------------------------------------------------------
        // Consume OTP
        // --------------------------------------------------------

        otpRequest.IsConsumed = true;
        otpRequest.ConsumedDate = DateTime.UtcNow;
        otpRequest.AttemptCount++;

        // --------------------------------------------------------
        // Verify Email
        // --------------------------------------------------------

        user.IsEmailVerified = true;
        user.ModifiedDate = DateTime.UtcNow;

        // --------------------------------------------------------
        // Invalidate Other Email Verification OTPs
        // --------------------------------------------------------

        var otherOtps =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == EmailVerificationPurpose &&
                    !x.IsConsumed &&
                    x.OTPRequestID != otpRequest.OTPRequestID)
                .ToListAsync();

        foreach (var otherOtp in otherOtps)
        {
            otherOtp.IsConsumed = true;
            otherOtp.ConsumedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // Issue Authentication Tokens
        // --------------------------------------------------------

        return await CreateAuthenticatedResponseAsync(
            user,
            "Email address verified successfully. You are now signed in.");
    }

    // ============================================================
    // RESEND EMAIL VERIFICATION OTP
    // ============================================================

    public async Task<bool> ResendEmailVerificationAsync(
        ResendEmailVerificationRequest request)
    {
        // --------------------------------------------------------
        // Validate Request
        // --------------------------------------------------------

        if (request == null ||
            string.IsNullOrWhiteSpace(request.Email))
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
        // Do Not Reveal Account Information
        // --------------------------------------------------------

        if (user == null)
        {
            return true;
        }

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // --------------------------------------------------------
        // Already Verified
        // --------------------------------------------------------

        if (user.IsEmailVerified)
        {
            return true;
        }

        // --------------------------------------------------------
        // Resend Cooldown
        // --------------------------------------------------------

        var latestOtp =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == EmailVerificationPurpose)
                .OrderByDescending(x =>
                    x.CreatedDate)
                .FirstOrDefaultAsync();

        if (latestOtp != null &&
            latestOtp.CreatedDate.AddSeconds(
                ResendCooldownSeconds) > DateTime.UtcNow)
        {
            return false;
        }

        // --------------------------------------------------------
        // Invalidate Existing Email Verification OTPs
        // --------------------------------------------------------

        var existingOtps =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == EmailVerificationPurpose &&
                    !x.IsConsumed)
                .ToListAsync();

        foreach (var existingOtp in existingOtps)
        {
            existingOtp.IsConsumed = true;
            existingOtp.ConsumedDate = DateTime.UtcNow;
        }

        // --------------------------------------------------------
        // Generate New OTP
        // --------------------------------------------------------

        var otp = GenerateOtp();

        var now = DateTime.UtcNow;

        var otpRequest = new OTPRequest
        {
            UserID = user.UserID,

            Destination = user.Email,

            Purpose = EmailVerificationPurpose,

            OTPHash =
                TokenHelper.HashToken(otp),

            ExpiresAt =
                now.AddMinutes(OtpExpiryMinutes),

            AttemptCount = 0,

            MaxAttempts = OtpMaxAttempts,

            IsConsumed = false,

            ConsumedDate = null,

            CreatedDate = now
        };

        _context.OTPRequests.Add(otpRequest);

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // Send Email
        // --------------------------------------------------------

        await SendEmailVerificationOtpAsync(
            user,
            otp);

        return true;
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
        // Defense-in-Depth:
        // Do not refresh tokens for an unverified email.
        // --------------------------------------------------------

        if (!user.IsEmailVerified)
        {
            return new AuthResponse
            {
                Success = false,
                Message =
                    "Email verification is required before authentication can continue.",
                RequiresEmailVerification = true,
                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email
            };
        }

        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var activeRoles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x => x.Role!.RoleName)
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
            Permissions = permissions,

            RequiresEmailVerification = false
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
                    x.Purpose == ForgotPasswordPurpose &&
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

        var otp = GenerateOtp();

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

            Purpose = ForgotPasswordPurpose,

            OTPHash = otpHash,

            ExpiresAt =
                DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),

            AttemptCount = 0,

            MaxAttempts = OtpMaxAttempts,

            IsConsumed = false,

            ConsumedDate = null,

            CreatedDate = DateTime.UtcNow
        };

        _context.OTPRequests.Add(
            otpRequest);

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // Send Password Reset Email
        //
        // Never log the OTP.
        // --------------------------------------------------------

        await SendPasswordResetOtpAsync(
            user,
            otp);

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
                    x.Purpose == ForgotPasswordPurpose &&
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
    // CHANGE PASSWORD
    // ============================================================

    public async Task<bool> ChangePasswordAsync(
        long userId,
        ChangePasswordRequest request)
    {
        // --------------------------------------------------------
        // Validate request
        // --------------------------------------------------------

        if (userId <= 0)
        {
            return false;
        }

        if (request == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
                request.CurrentPassword))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
                request.NewPassword))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
                request.ConfirmPassword))
        {
            return false;
        }

        if (!string.Equals(
                request.NewPassword,
                request.ConfirmPassword,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (request.NewPassword.Length < 8)
        {
            return false;
        }

        if (string.Equals(
                request.CurrentPassword,
                request.NewPassword,
                StringComparison.Ordinal))
        {
            return false;
        }

        // --------------------------------------------------------
        // Find User
        // --------------------------------------------------------

        var user =
            await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId);

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
        // Verify Current Password
        // --------------------------------------------------------

        var currentPasswordValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.CurrentPassword);

        if (!currentPasswordValid)
        {
            return false;
        }

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
            .Select(x =>
                x.Role!.RoleName)
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

            Permissions = permissions,

            RequiresEmailVerification =
                !user.IsEmailVerified
        };
    }

    // ============================================================
    // PRIVATE: GENERATE OTP
    // ============================================================

    private static string GenerateOtp()
    {
        return Random.Shared
            .Next(100000, 1000000)
            .ToString();
    }

    // ============================================================
    // PRIVATE: CREATE EMAIL VERIFICATION OTP
    // ============================================================

    private async Task<string> CreateEmailVerificationOtpAsync(
        User user)
    {
        // --------------------------------------------------------
        // Invalidate Existing Email Verification OTPs
        // --------------------------------------------------------

        var existingOtps =
            await _context.OTPRequests
                .Where(x =>
                    x.UserID == user.UserID &&
                    x.Purpose == EmailVerificationPurpose &&
                    !x.IsConsumed)
                .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var existingOtp in existingOtps)
        {
            existingOtp.IsConsumed = true;
            existingOtp.ConsumedDate = now;
        }

        // --------------------------------------------------------
        // Generate OTP
        // --------------------------------------------------------

        var otp = GenerateOtp();

        // --------------------------------------------------------
        // Store Hash Only
        // --------------------------------------------------------

        var otpRequest = new OTPRequest
        {
            UserID = user.UserID,

            Destination = user.Email,

            Purpose = EmailVerificationPurpose,

            OTPHash =
                TokenHelper.HashToken(otp),

            ExpiresAt =
                now.AddMinutes(OtpExpiryMinutes),

            AttemptCount = 0,

            MaxAttempts = OtpMaxAttempts,

            IsConsumed = false,

            ConsumedDate = null,

            CreatedDate = now
        };

        _context.OTPRequests.Add(otpRequest);

        await _context.SaveChangesAsync();

        return otp;
    }

    // ============================================================
    // PRIVATE: SEND EMAIL VERIFICATION OTP
    // ============================================================

    private async Task SendEmailVerificationOtpAsync(
        User user,
        string otp)
    {
        var firstName =
            user.UserProfile?.FirstName;

        var displayName =
            string.IsNullOrWhiteSpace(firstName)
                ? user.UserName
                : firstName;

        var subject =
            "Verify your HR INFO ADs email address";

        var htmlBody = BuildVerificationEmail(
            displayName,
            otp);

        await _emailService.SendAsync(
            user.Email,
            subject,
            htmlBody);
    }

    // ============================================================
    // PRIVATE: SEND PASSWORD RESET OTP
    // ============================================================

    private async Task SendPasswordResetOtpAsync(
        User user,
        string otp)
    {
        var firstName =
            user.UserProfile?.FirstName;

        var displayName =
            string.IsNullOrWhiteSpace(firstName)
                ? user.UserName
                : firstName;

        var subject =
            "Your HR INFO ADs password reset code";

        var htmlBody = BuildPasswordResetEmail(
            displayName,
            otp);

        await _emailService.SendAsync(
            user.Email,
            subject,
            htmlBody);
    }

    // ============================================================
    // PRIVATE: CREATE AUTHENTICATED RESPONSE
    // ============================================================

    private async Task<AuthResponse> CreateAuthenticatedResponseAsync(
        User user,
        string message)
    {
        // --------------------------------------------------------
        // Load Active Roles
        // --------------------------------------------------------

        var activeRoles = user.UserRoles
            .Where(x =>
                x.Role != null &&
                x.Role.IsActive)
            .Select(x =>
                x.Role!.RoleName)
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

        var refreshTokenEntity =
            new RefreshToken
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
        // Return
        // --------------------------------------------------------

        return new AuthResponse
        {
            Success = true,

            Message = message,

            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,

            AccessToken = accessToken,
            RefreshToken = refreshToken,

            ExpiresAt =
                _jwtService.GetAccessTokenExpiration(),

            Roles = activeRoles,
            Permissions = permissions,

            RequiresEmailVerification = false
        };
    }

    // ============================================================
    // PRIVATE: VERIFICATION EMAIL HTML
    // ============================================================

    private static string BuildVerificationEmail(
        string displayName,
        string otp)
    {
        var safeName =
            System.Net.WebUtility.HtmlEncode(displayName);

        return $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Verify your HR INFO ADs account</title>
</head>
<body style="margin:0;padding:0;background:#f5f3f0;font-family:Arial,Helvetica,sans-serif;color:#333333;">

    <div style="width:100%;padding:40px 15px;box-sizing:border-box;">

        <div style="max-width:600px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 18px rgba(0,0,0,0.08);">

            <div style="background:#663333;padding:28px 30px;text-align:center;">
                <div style="font-size:28px;font-weight:700;color:#ffffff;">
                    HR INFO ADs
                </div>
                <div style="font-size:14px;color:#ffbf00;margin-top:6px;">
                    Your marketplace for everything
                </div>
            </div>

            <div style="padding:35px 30px;">

                <h1 style="margin:0 0 18px;font-size:24px;color:#663333;">
                    Verify your email address
                </h1>

                <p style="margin:0 0 15px;font-size:15px;line-height:1.7;">
                    Hello {safeName},
                </p>

                <p style="margin:0 0 25px;font-size:15px;line-height:1.7;">
                    Thank you for creating your HR INFO ADs account.
                    Please use the verification code below to verify
                    your email address.
                </p>

                <div style="text-align:center;margin:30px 0;">
                    <div style="display:inline-block;padding:18px 35px;background:#f8f5ef;border:2px solid #ffbf00;border-radius:10px;">
                        <span style="font-size:32px;font-weight:700;letter-spacing:8px;color:#663333;">
                            {otp}
                        </span>
                    </div>
                </div>

                <p style="margin:0 0 10px;text-align:center;font-size:14px;color:#666666;">
                    This verification code is valid for <strong>10 minutes</strong>.
                </p>

                <p style="margin:25px 0 0;font-size:13px;line-height:1.6;color:#777777;">
                    If you did not create this account, you can safely ignore
                    this email.
                </p>

            </div>

            <div style="background:#f8f8f8;padding:20px 30px;text-align:center;">
                <p style="margin:0;font-size:12px;color:#888888;">
                    © HR INFO ADs. All rights reserved.
                </p>
            </div>

        </div>

    </div>

</body>
</html>
""";
    }

    // ============================================================
    // PRIVATE: PASSWORD RESET EMAIL HTML
    // ============================================================

    private static string BuildPasswordResetEmail(
        string displayName,
        string otp)
    {
        var safeName =
            System.Net.WebUtility.HtmlEncode(displayName);

        return $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>HR INFO ADs password reset</title>
</head>
<body style="margin:0;padding:0;background:#f5f3f0;font-family:Arial,Helvetica,sans-serif;color:#333333;">

    <div style="width:100%;padding:40px 15px;box-sizing:border-box;">

        <div style="max-width:600px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 18px rgba(0,0,0,0.08);">

            <div style="background:#663333;padding:28px 30px;text-align:center;">
                <div style="font-size:28px;font-weight:700;color:#ffffff;">
                    HR INFO ADs
                </div>
                <div style="font-size:14px;color:#ffbf00;margin-top:6px;">
                    Your marketplace for everything
                </div>
            </div>

            <div style="padding:35px 30px;">

                <h1 style="margin:0 0 18px;font-size:24px;color:#663333;">
                    Password reset code
                </h1>

                <p style="margin:0 0 15px;font-size:15px;line-height:1.7;">
                    Hello {safeName},
                </p>

                <p style="margin:0 0 25px;font-size:15px;line-height:1.7;">
                    We received a request to reset the password for your
                    HR INFO ADs account. Use the code below to continue.
                </p>

                <div style="text-align:center;margin:30px 0;">
                    <div style="display:inline-block;padding:18px 35px;background:#f8f5ef;border:2px solid #ffbf00;border-radius:10px;">
                        <span style="font-size:32px;font-weight:700;letter-spacing:8px;color:#663333;">
                            {otp}
                        </span>
                    </div>
                </div>

                <p style="margin:0 0 10px;text-align:center;font-size:14px;color:#666666;">
                    This code is valid for <strong>10 minutes</strong>.
                </p>

                <p style="margin:25px 0 0;font-size:13px;line-height:1.6;color:#777777;">
                    If you did not request a password reset, please ignore
                    this email. Your password will remain unchanged.
                </p>

            </div>

            <div style="background:#f8f8f8;padding:20px 30px;text-align:center;">
                <p style="margin:0;font-size:12px;color:#888888;">
                    © HR INFO ADs. All rights reserved.
                </p>
            </div>

        </div>

    </div>

</body>
</html>
""";
    }
}