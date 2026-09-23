namespace HRInfoAdvertisements.Application.Messaging.DTOs;

public class MarkMessageReadResponse
{
    public long MessageID { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }
}