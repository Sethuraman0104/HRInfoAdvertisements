namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementEnquiry
{
    public long AdvertisementEnquiryID { get; set; }

    public long AdvertisementID { get; set; }

    public long SenderUserID { get; set; }

    public long RecipientUserID { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? ContactMobile { get; set; }

    public string? ContactEmail { get; set; }

    public string Status { get; set; } = "OPEN";

    public DateTime CreatedDate { get; set; }

    public DateTime? LastRepliedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public User SenderUser { get; set; } = null!;

    public User RecipientUser { get; set; } = null!;

    public ICollection<AdvertisementEnquiryMessage> Messages { get; set; }
        = new List<AdvertisementEnquiryMessage>();
}