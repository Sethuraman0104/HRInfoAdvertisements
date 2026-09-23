namespace HRInfoAdvertisements.Application.Enquiries.DTOs;

public class CreateAdvertisementEnquiryRequest
{
    public long AdvertisementID { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? ContactMobile { get; set; }

    public string? ContactEmail { get; set; }
}