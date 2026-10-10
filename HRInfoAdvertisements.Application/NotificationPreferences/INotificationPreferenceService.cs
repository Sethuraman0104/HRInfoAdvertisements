using HRInfoAdvertisements.Application.NotificationPreferences.DTOs;

namespace HRInfoAdvertisements.Application.NotificationPreferences;

public interface INotificationPreferenceService
{
    Task<NotificationPreferenceResponse> GetAsync(long userId);

    Task<NotificationPreferenceResponse> UpdateAsync(
        long userId,
        UpdateNotificationPreferenceRequest request);
}