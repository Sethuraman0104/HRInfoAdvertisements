namespace HRInfoAdvertisements.Application.Messaging.DTOs;

public class MessageResponse
{
    public long MessageID { get; set; }

    public long SenderUserID { get; set; }

    public string SenderUserName { get; set; } = string.Empty;

    public long ReceiverUserID { get; set; }

    public string ReceiverUserName { get; set; } = string.Empty;

    public long? AdvertisementID { get; set; }

    public long? EnquiryID { get; set; }

    public string MessageText { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public DateTime CreatedDate { get; set; }
}