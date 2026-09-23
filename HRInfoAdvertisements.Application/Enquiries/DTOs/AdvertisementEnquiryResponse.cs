namespace HRInfoAdvertisements.Application.Enquiries.DTOs;

public class AdvertisementEnquiryResponse
{
    public long AdvertisementEnquiryID { get; set; }

    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public string AdvertisementTitle { get; set; } = string.Empty;

    public long SenderUserID { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public long RecipientUserID { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime? LastRepliedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public int MessageCount { get; set; }
}