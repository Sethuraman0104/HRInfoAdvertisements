using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class AdminAdvertisementDetailResponse
{
    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string AdvertisementTypeName { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? LandArea { get; set; }

    public decimal? BuiltUpArea { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? PropertyAge { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? FeaturedUntil { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public List<AdvertisementImageResponse> Images { get; set; } = new();

    public List<AdvertisementVideoResponse> Videos { get; set; } = new();

    public List<AdvertisementDocumentResponse> Documents { get; set; } = new();

    public List<ApprovalHistoryResponse> ApprovalHistory { get; set; } = new();
}