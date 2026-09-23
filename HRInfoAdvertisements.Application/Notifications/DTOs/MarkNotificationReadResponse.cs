namespace HRInfoAdvertisements.Application.Notifications.DTOs;

public class MarkNotificationReadResponse
{
    public long NotificationID { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }
}