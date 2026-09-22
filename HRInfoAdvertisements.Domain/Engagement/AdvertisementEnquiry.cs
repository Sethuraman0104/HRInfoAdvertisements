namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementEnquiry
{
    public long AdvertisementEnquiryID { get; set; }

    public long AdvertisementID { get; set; }

    public long SenderUserID { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? ContactMobile { get; set; }

    public string? ContactEmail { get; set; }

    public string Status { get; set; } = "New";

    public DateTime CreatedDate { get; set; }

    public DateTime? RespondedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public User SenderUser { get; set; } = null!;
}