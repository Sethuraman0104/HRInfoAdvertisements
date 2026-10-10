using HRInfoAdvertisements.Application.DTOs.Email;
using HRInfoAdvertisements.Application.DTOs.Settings;
using HRInfoAdvertisements.Application.Services;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public sealed class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly ApplicationDbContext _context;

    public ApplicationSettingsService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetStringAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return null;
        }

        return await _context.SystemSettings
            .AsNoTracking()
            .Where(x =>
                x.SettingKey == settingKey &&
                x.IsActive)
            .Select(x => x.SettingValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string> GetRequiredStringAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        var value = await GetStringAsync(
            settingKey,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required system setting '{settingKey}' is missing or inactive.");
        }

        return value.Trim();
    }

    public async Task<int?> GetIntAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        var value = await GetStringAsync(
            settingKey,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(
            value,
            out var result)
                ? result
                : null;
    }

    public async Task<bool?> GetBoolAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        var value = await GetStringAsync(
            settingKey,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.TryParse(
            value,
            out var result)
                ? result
                : null;
    }

    public async Task<AdMindEmailBranding> GetEmailBrandingAsync(
        CancellationToken cancellationToken = default)
    {
        var settings =
            await GetApplicationSettingsAsync(cancellationToken);

        return new AdMindEmailBranding
        {
            SiteName = settings.SiteName,
            SiteTagline = settings.SiteTagline,
            SiteUrl = settings.SiteUrl,
            LogoUrl = settings.EmailLogoUrl,
            SupportEmail = settings.SupportEmail,
            FromName = settings.EmailFromName,
            VerificationSubject =
                settings.EmailVerificationSubject,
            PasswordResetSubject =
                settings.EmailPasswordResetSubject,
            OtpExpiryMinutes =
                settings.EmailOtpExpiryMinutes,
            EmailEnabled =
                settings.EmailEnabled
        };
    }

    public async Task<ApplicationSettings> GetApplicationSettingsAsync(
    CancellationToken cancellationToken = default)
{
    return new ApplicationSettings
    {
        SiteName =
            await GetStringAsync(
                "SITE_NAME",
                cancellationToken)
            ?? "AdMind",

        DefaultCurrency =
            await GetStringAsync(
                "DEFAULT_CURRENCY",
                cancellationToken)
            ?? "BHD",

        AdvertisementExpiryDays =
            await GetIntAsync(
                "ADVERTISEMENT_EXPIRY_DAYS",
                cancellationToken)
            ?? 30,

        MaxImagesPerAdvertisement =
            await GetIntAsync(
                "MAX_IMAGES_PER_ADVERTISEMENT",
                cancellationToken)
            ?? 2,

        MaxVideoSizeMb =
            await GetIntAsync(
                "MAX_VIDEO_SIZE_MB",
                cancellationToken)
            ?? 100,

        RequireAdminApproval =
            await GetBoolAsync(
                "REQUIRE_ADMIN_APPROVAL",
                cancellationToken)
            ?? true,

        RequireEmailVerification =
            await GetBoolAsync(
                "REQUIRE_EMAIL_VERIFICATION",
                cancellationToken)
            ?? true,

        RequireMobileVerification =
            await GetBoolAsync(
                "REQUIRE_MOBILE_VERIFICATION",
                cancellationToken)
            ?? true,

        AdminMfaRequired =
            await GetBoolAsync(
                "ADMIN_MFA_REQUIRED",
                cancellationToken)
            ?? true,

        SiteTagline =
            await GetStringAsync(
                "SITE_TAGLINE",
                cancellationToken)
            ?? "Your marketplace for everything",

        SiteUrl =
            await GetStringAsync(
                "SITE_URL",
                cancellationToken)
            ?? string.Empty,

        EmailLogoUrl =
            await GetStringAsync(
                "EMAIL_LOGO_URL",
                cancellationToken)
            ?? string.Empty,

        SupportEmail =
            await GetStringAsync(
                "SUPPORT_EMAIL",
                cancellationToken)
            ?? string.Empty,

        EmailFromName =
            await GetStringAsync(
                "EMAIL_FROM_NAME",
                cancellationToken)
            ?? "AdMind",

        EmailVerificationSubject =
            await GetStringAsync(
                "EMAIL_VERIFICATION_SUBJECT",
                cancellationToken)
            ?? "Verify your AdMind account",

        EmailPasswordResetSubject =
            await GetStringAsync(
                "EMAIL_PASSWORD_RESET_SUBJECT",
                cancellationToken)
            ?? "Reset your AdMind password",

        EmailOtpExpiryMinutes =
            await GetIntAsync(
                "EMAIL_OTP_EXPIRY_MINUTES",
                cancellationToken)
            ?? 10,

        
EmailEnabled =
    await GetBoolAsync(
        "EMAIL_ENABLED",
        cancellationToken)
    ?? true,

AutomatedEmailEnabled =
    await GetBoolAsync(
        "AUTOMATED_EMAIL_ENABLED",
        cancellationToken)
    ?? false,

AccountEmailEnabled =
    await GetBoolAsync(
        "ACCOUNT_EMAIL_ENABLED",
        cancellationToken)
    ?? true,

AdvertisementEmailEnabled =
    await GetBoolAsync(
        "ADVERTISEMENT_EMAIL_ENABLED",
        cancellationToken)
    ?? true,

FavoriteAdvertisementEmailEnabled =
    await GetBoolAsync(
        "FAVORITE_ADVERTISEMENT_EMAIL_ENABLED",
        cancellationToken)
    ?? true
    };
}

    private static string GetString(
        IDictionary<string, string?> values,
        string key,
        string defaultValue)
    {
        if (!values.TryGetValue(key, out var value) ||
            string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Trim();
    }

    private static int GetPositiveInt(
        IDictionary<string, string?> values,
        string key,
        int defaultValue)
    {
        if (!values.TryGetValue(key, out var value) ||
            !int.TryParse(value, out var result) ||
            result <= 0)
        {
            return defaultValue;
        }

        return result;
    }

    private static bool GetBool(
        IDictionary<string, string?> values,
        string key,
        bool defaultValue)
    {
        if (!values.TryGetValue(key, out var value) ||
            !bool.TryParse(value, out var result))
        {
            return defaultValue;
        }

        return result;
    }
}