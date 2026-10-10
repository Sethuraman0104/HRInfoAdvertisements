namespace HRInfoAdvertisements.Application.DTOs.Settings;

public sealed class ApplicationSettings
{
    public string SiteName { get; init; } = "AdMind";

    public string DefaultCurrency { get; init; } = "BHD";

    public int AdvertisementExpiryDays { get; init; } = 30;

    public int MaxImagesPerAdvertisement { get; init; } = 2;

    public int MaxVideoSizeMb { get; init; } = 100;

    public bool RequireAdminApproval { get; init; } = true;

    public bool RequireEmailVerification { get; init; } = true;

    public bool RequireMobileVerification { get; init; } = true;

    public bool AdminMfaRequired { get; init; } = true;

    public string SiteTagline { get; init; } =
        "Your marketplace for everything";

    public string SiteUrl { get; init; } = string.Empty;

    public string EmailLogoUrl { get; init; } = string.Empty;

    public string SupportEmail { get; init; } = string.Empty;

    public string EmailFromName { get; init; } = "AdMind";

    public string EmailVerificationSubject { get; init; } =
        "Verify your AdMind account";

    public string EmailPasswordResetSubject { get; init; } =
        "Reset your AdMind password";

    public int EmailOtpExpiryMinutes { get; init; } = 10;

    public bool EmailEnabled { get; init; } = true;

    // Optional marketplace email automation controls.
// These do not control verification, OTP, or password-reset emails.
public bool AutomatedEmailEnabled { get; init; } = false;

public bool AccountEmailEnabled { get; init; } = true;

public bool AdvertisementEmailEnabled { get; init; } = true;

public bool FavoriteAdvertisementEmailEnabled { get; init; } = true;
}