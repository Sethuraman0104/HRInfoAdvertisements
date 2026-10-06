// HRInfoAdvertisements.Application/DTOs/SystemSettings/PublicSiteSettingsResponse.cs
namespace HRInfoAdvertisements.Application.DTOs.SystemSettings;

public sealed class PublicSiteSettingsResponse
{
    public string SiteName { get; set; } = "AdMind";
    public string SiteTagline { get; set; } = "Your marketplace for everything";
    public string DefaultCurrency { get; set; } = "INR";
    public int AdvertisementExpiryDays { get; set; } = 30;
    public int MaxImagesPerAdvertisement { get; set; } = 5;
    public int MaxVideoSizeMb { get; set; } = 100;
    public bool RequireAdminApproval { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public int EmailOtpExpiryMinutes { get; set; } = 10;
    public string SupportEmail { get; set; } = string.Empty;
}