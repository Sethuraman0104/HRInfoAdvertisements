using HRInfoAdvertisements.Application.DTOs.Email;
using HRInfoAdvertisements.Application.DTOs.Settings;

namespace HRInfoAdvertisements.Application.Services;

public interface IApplicationSettingsService
{
    Task<string?> GetStringAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<string> GetRequiredStringAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<int?> GetIntAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<bool?> GetBoolAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<ApplicationSettings> GetApplicationSettingsAsync(
        CancellationToken cancellationToken = default);

    Task<AdMindEmailBranding> GetEmailBrandingAsync(
        CancellationToken cancellationToken = default);
}