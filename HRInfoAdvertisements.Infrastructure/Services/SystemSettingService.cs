using HRInfoAdvertisements.Application.DTOs.Settings;
using HRInfoAdvertisements.Application.DTOs.SystemSettings;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Security;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfigurationEncryptionService _encryptionService;

    public SystemSettingService(
        ApplicationDbContext context,
        IConfigurationEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    // ============================================================
    // GET SYSTEM SETTINGS
    // ============================================================

    public async Task<SystemSettingListResponse>
        GetSystemSettingsAsync(
            SystemSettingListRequest request)
    {
        var pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        var pageSize =
            request.PageSize switch
            {
                <= 0 => 50,
                > 100 => 100,
                _ => request.PageSize
            };

        var query =
            _context.SystemSettings
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search =
                request.Search.Trim();

            query = query.Where(x =>
                x.SettingKey.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == request.IsActive.Value);
        }

        var totalRecords =
            await query.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        if (totalPages > 0 &&
            pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var settings =
            await query
                .OrderBy(x => x.SettingKey)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        var items =
            settings
                .Select(x =>
                    new SystemSettingListItemResponse
                    {
                        SystemSettingID =
                            x.SystemSettingID,

                        SettingKey =
                            x.SettingKey,

                        // Never expose encrypted values.
                        SettingValue =
                            x.IsEncrypted
                                ? null
                                : x.SettingValue,

                        Description =
                            x.Description,

                        IsEncrypted =
                            x.IsEncrypted,

                        IsActive =
                            x.IsActive,

                        CreatedDate =
                            x.CreatedDate,

                        ModifiedDate =
                            x.ModifiedDate
                    })
                .ToList();

        return new SystemSettingListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // ============================================================
    // GET SYSTEM SETTING BY ID
    // ============================================================

    public async Task<SystemSettingDetailResponse?>
        GetSystemSettingByIdAsync(
            int systemSettingId)
    {
        var setting =
            await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.SystemSettingID ==
                    systemSettingId);

        if (setting == null)
        {
            return null;
        }

        return new SystemSettingDetailResponse
        {
            SystemSettingID =
                setting.SystemSettingID,

            SettingKey =
                setting.SettingKey,

            // Never expose encrypted values.
            SettingValue =
                setting.IsEncrypted
                    ? null
                    : setting.SettingValue,

            Description =
                setting.Description,

            IsEncrypted =
                setting.IsEncrypted,

            IsActive =
                setting.IsActive,

            CreatedDate =
                setting.CreatedDate,

            ModifiedDate =
                setting.ModifiedDate
        };
    }

    // ============================================================
    // CREATE SYSTEM SETTING
    // ============================================================

    public async Task<int?>
        CreateSystemSettingAsync(
            CreateSystemSettingRequest request,
            long createdBy)
    {
        var settingKey =
            request.SettingKey.Trim();

        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return null;
        }

        var exists =
            await _context.SystemSettings
                .AnyAsync(x =>
                    x.SettingKey.ToLower() ==
                    settingKey.ToLower());

        if (exists)
        {
            return null;
        }

        var settingValue =
            string.IsNullOrWhiteSpace(
                request.SettingValue)
                ? null
                : request.SettingValue.Trim();

        // Encrypt sensitive settings before they are persisted.
        if (request.IsEncrypted &&
            !string.IsNullOrWhiteSpace(settingValue))
        {
            settingValue =
                _encryptionService.Protect(
                    settingValue);
        }

        var setting =
            new SystemSetting
            {
                SettingKey =
                    settingKey,

                SettingValue =
                    settingValue,

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Description)
                        ? null
                        : request.Description.Trim(),

                IsEncrypted =
                    request.IsEncrypted,

                IsActive =
                    request.IsActive,

                CreatedDate =
                    DateTime.UtcNow
            };

        _context.SystemSettings.Add(setting);

        await _context.SaveChangesAsync();

        return setting.SystemSettingID;
    }

    // ============================================================
    // UPDATE SYSTEM SETTING
    // ============================================================

    public async Task<bool>
        UpdateSystemSettingAsync(
            int systemSettingId,
            UpdateSystemSettingRequest request,
            long modifiedBy)
    {
        var setting =
            await _context.SystemSettings
                .FirstOrDefaultAsync(x =>
                    x.SystemSettingID ==
                    systemSettingId);

        if (setting == null)
        {
            return false;
        }

        var settingKey =
            request.SettingKey.Trim();

        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return false;
        }

        var duplicateKey =
            await _context.SystemSettings
                .AnyAsync(x =>
                    x.SystemSettingID !=
                        systemSettingId &&
                    x.SettingKey.ToLower() ==
                        settingKey.ToLower());

        if (duplicateKey)
        {
            return false;
        }

        setting.SettingKey =
            settingKey;

        // ========================================================
        // VALUE HANDLING
        // ========================================================

        var newValue =
            string.IsNullOrWhiteSpace(
                request.SettingValue)
                ? null
                : request.SettingValue.Trim();

        if (request.IsEncrypted)
        {
            // When editing an existing encrypted setting, the UI
            // intentionally sends an empty value when the user
            // does not want to replace the secret.
            //
            // Preserve the existing encrypted value in that case.
            if (!string.IsNullOrWhiteSpace(newValue))
            {
                setting.SettingValue =
                    _encryptionService.Protect(
                        newValue);
            }
            else if (!setting.IsEncrypted)
            {
                // The setting is being changed from plaintext
                // to encrypted but no new secret was supplied.
                // Do not retain the old plaintext value.
                setting.SettingValue = null;
            }

            // If it was already encrypted and the new value is
            // empty, leave the existing encrypted value untouched.
        }
        else
        {
            // Setting is being stored as plaintext.
            // The caller explicitly changed IsEncrypted to false,
            // so store the supplied plaintext value.
            setting.SettingValue =
                newValue;
        }

        setting.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        setting.IsEncrypted =
            request.IsEncrypted;

        setting.IsActive =
            request.IsActive;

        setting.ModifiedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // UPDATE STATUS
    // ============================================================

    public async Task<bool>
        UpdateSystemSettingStatusAsync(
            int systemSettingId,
            bool isActive,
            long modifiedBy)
    {
        var setting =
            await _context.SystemSettings
                .FirstOrDefaultAsync(x =>
                    x.SystemSettingID ==
                    systemSettingId);

        if (setting == null)
        {
            return false;
        }

        setting.IsActive =
            isActive;

        setting.ModifiedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
// GET APPLICATION SETTINGS
// ============================================================

public async Task<ApplicationSettings>
    GetApplicationSettingsAsync(
        CancellationToken cancellationToken = default)
{
    var settings =
        await _context.SystemSettings
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToDictionaryAsync(
                x => x.SettingKey,
                x => x.SettingValue,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

    return new ApplicationSettings
    {
        SiteName =
            GetString(
                settings,
                "SITE_NAME",
                "AdMind"),

        DefaultCurrency =
            GetString(
                settings,
                "DEFAULT_CURRENCY",
                "BHD"),

        AdvertisementExpiryDays =
            GetInt(
                settings,
                "ADVERTISEMENT_EXPIRY_DAYS",
                30),

        MaxImagesPerAdvertisement =
            GetInt(
                settings,
                "MAX_IMAGES_PER_ADVERTISEMENT",
                2),

        MaxVideoSizeMb =
            GetInt(
                settings,
                "MAX_VIDEO_SIZE_MB",
                100),

        RequireAdminApproval =
            GetBool(
                settings,
                "REQUIRE_ADMIN_APPROVAL",
                true),

        RequireEmailVerification =
            GetBool(
                settings,
                "REQUIRE_EMAIL_VERIFICATION",
                true),

        RequireMobileVerification =
            GetBool(
                settings,
                "REQUIRE_MOBILE_VERIFICATION",
                true),

        AdminMfaRequired =
            GetBool(
                settings,
                "ADMIN_MFA_REQUIRED",
                true),

        SiteTagline =
            GetString(
                settings,
                "SITE_TAGLINE",
                "Your marketplace for everything"),

        SiteUrl =
            GetString(
                settings,
                "SITE_URL",
                string.Empty),

        EmailLogoUrl =
            GetString(
                settings,
                "EMAIL_LOGO_URL",
                string.Empty),

        SupportEmail =
            GetString(
                settings,
                "SUPPORT_EMAIL",
                string.Empty),

        EmailFromName =
            GetString(
                settings,
                "EMAIL_FROM_NAME",
                "AdMind"),

        EmailVerificationSubject =
            GetString(
                settings,
                "EMAIL_VERIFICATION_SUBJECT",
                "Verify your AdMind account"),

        EmailPasswordResetSubject =
            GetString(
                settings,
                "EMAIL_PASSWORD_RESET_SUBJECT",
                "Reset your AdMind password"),

        EmailOtpExpiryMinutes =
            GetInt(
                settings,
                "EMAIL_OTP_EXPIRY_MINUTES",
                10),

        EmailEnabled =
            GetBool(
                settings,
                "EMAIL_ENABLED",
                true)
    };
}

    // ============================================================
    // GET DECRYPTED VALUE
    // ============================================================
    //
    // Internal application use only.
    //
    // This method is intentionally NOT part of the Admin API
    // response. It is for trusted server-side consumers such as
    // the future email configuration resolver.
    //
    // ============================================================

    public async Task<string?>
        GetDecryptedSettingValueAsync(
            string settingKey)
    {
        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return null;
        }

        var setting =
            await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.SettingKey == settingKey &&
                    x.IsActive);

        if (setting == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
                setting.SettingValue))
        {
            return null;
        }

        if (!setting.IsEncrypted)
        {
            return setting.SettingValue;
        }

        try
        {
            return _encryptionService.Unprotect(
                setting.SettingValue);
        }
        catch
        {
            // Do not expose encrypted/ciphertext details to callers.
            // A future logging layer can record the failure safely.
            return null;
        }
    }

    // ============================================================
// APPLICATION SETTINGS HELPERS
// ============================================================

private static string GetString(
    IReadOnlyDictionary<string, string?> settings,
    string key,
    string defaultValue)
{
    if (!settings.TryGetValue(key, out var value) ||
        string.IsNullOrWhiteSpace(value))
    {
        return defaultValue;
    }

    return value.Trim();
}

private static int GetInt(
    IReadOnlyDictionary<string, string?> settings,
    string key,
    int defaultValue)
{
    if (!settings.TryGetValue(key, out var value) ||
        string.IsNullOrWhiteSpace(value))
    {
        return defaultValue;
    }

    return int.TryParse(
        value.Trim(),
        out var result)
            ? result
            : defaultValue;
}

private static bool GetBool(
    IReadOnlyDictionary<string, string?> settings,
    string key,
    bool defaultValue)
{
    if (!settings.TryGetValue(key, out var value) ||
        string.IsNullOrWhiteSpace(value))
    {
        return defaultValue;
    }

    return bool.TryParse(
        value.Trim(),
        out var result)
            ? result
            : defaultValue;
}
}