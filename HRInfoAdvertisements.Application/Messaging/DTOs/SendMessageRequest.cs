namespace HRInfoAdvertisements.Application.Messaging.DTOs;

public class SendMessageRequest
{
    public long ReceiverUserID { get; set; }

    public long? AdvertisementID { get; set; }

    public long? EnquiryID { get; set; }

    public string MessageText { get; set; } = string.Empty;
}