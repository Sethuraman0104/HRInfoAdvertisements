namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class AdminAdvertisementListResponse
{
    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public string CategoryName { get; set; } = string.Empty;

    public string AdvertisementTypeName { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }
}