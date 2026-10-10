namespace HRInfoAdvertisements.Application.NotificationPreferences.DTOs;

public class NotificationPreferenceResponse
{
    public long UserID { get; set; }

    public bool EmailNotificationsEnabled { get; set; }

    public bool AdvertisementUpdatesEnabled { get; set; }

    public bool FavoriteAdvertisementUpdatesEnabled { get; set; }
}