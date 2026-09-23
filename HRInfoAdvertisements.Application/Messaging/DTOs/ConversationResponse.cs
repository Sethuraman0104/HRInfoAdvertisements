namespace HRInfoAdvertisements.Application.Messaging.DTOs;

public class ConversationResponse
{
    public long OtherUserID { get; set; }

    public string OtherUserName { get; set; } = string.Empty;

    public long? AdvertisementID { get; set; }

    public long? EnquiryID { get; set; }

    public string LastMessage { get; set; } = string.Empty;

    public DateTime LastMessageDate { get; set; }

    public int UnreadCount { get; set; }
}