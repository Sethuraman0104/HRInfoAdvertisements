namespace HRInfoAdvertisements.Domain.Entities;

public class NotificationPreference
{
    public long NotificationPreferenceID { get; set; }

    public long UserID { get; set; }

    public bool EmailNotificationsEnabled { get; set; } = true;

    public bool AdvertisementUpdatesEnabled { get; set; } = true;

    public bool FavoriteAdvertisementUpdatesEnabled { get; set; } = true;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;
}