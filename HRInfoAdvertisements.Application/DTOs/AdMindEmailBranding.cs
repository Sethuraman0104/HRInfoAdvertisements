namespace HRInfoAdvertisements.Application.DTOs.Email;

public sealed class AdMindEmailBranding
{
    public string SiteName { get; init; } = "AdMind";

    public string SiteTagline { get; init; } =
        "Your marketplace for everything";

    public string SiteUrl { get; init; } = string.Empty;

    public string LogoUrl { get; init; } = string.Empty;

    public string SupportEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = "AdMind";

    public string VerificationSubject { get; init; } =
        "Verify your AdMind account";

    public string PasswordResetSubject { get; init; } =
        "Reset your AdMind password";

    public int OtpExpiryMinutes { get; init; } = 10;

    public bool EmailEnabled { get; init; } = true;
}