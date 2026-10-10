namespace HRInfoAdvertisements.Application.NotificationPreferences.DTOs;

public class UpdateNotificationPreferenceRequest
{
    public bool EmailNotificationsEnabled { get; set; }

    public bool AdvertisementUpdatesEnabled { get; set; }

    public bool FavoriteAdvertisementUpdatesEnabled { get; set; }
}