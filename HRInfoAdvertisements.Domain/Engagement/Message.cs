namespace HRInfoAdvertisements.Domain.Entities;

public class Message
{
    public long MessageID { get; set; }

    public long SenderUserID { get; set; }

    public long ReceiverUserID { get; set; }

    public long? AdvertisementID { get; set; }

    public long? EnquiryID { get; set; }

    public string MessageText { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public User SenderUser { get; set; } = null!;

    public User ReceiverUser { get; set; } = null!;

    public Advertisement? Advertisement { get; set; }

    public AdvertisementEnquiry? Enquiry { get; set; }
}