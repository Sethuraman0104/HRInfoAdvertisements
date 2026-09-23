namespace HRInfoAdvertisements.Application.Notifications.DTOs;

public class NotificationResponse
{
    public long NotificationID { get; set; }

    public long UserID { get; set; }

    public int? NotificationTemplateID { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceID { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public DateTime CreatedDate { get; set; }
}