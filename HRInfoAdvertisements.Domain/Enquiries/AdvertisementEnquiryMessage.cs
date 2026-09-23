namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementEnquiryMessage
{
    public long AdvertisementEnquiryMessageID { get; set; }

    public long AdvertisementEnquiryID { get; set; }

    public long SenderUserID { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public AdvertisementEnquiry AdvertisementEnquiry { get; set; }
        = null!;

    public User SenderUser { get; set; } = null!;
}