namespace HRInfoAdvertisements.Application.Enquiries.DTOs;

public class AdvertisementEnquiryMessageResponse
{
    public long AdvertisementEnquiryMessageID { get; set; }

    public long AdvertisementEnquiryID { get; set; }

    public long SenderUserID { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }
}