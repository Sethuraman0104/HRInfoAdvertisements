using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class AdminAdvertisementDetailResponse
{
    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    // ============================================================
    // OWNER
    // ============================================================

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    // ============================================================
    // BASIC INFORMATION
    // ============================================================

    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int AdvertisementTypeID { get; set; }

    public string AdvertisementTypeName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    // ============================================================
    // CONTACT INFORMATION
    // ============================================================

    public string? WhatsAppNumber { get; set; }

    public string? ContactEmail { get; set; }

    public bool ShowWhatsAppToPublic { get; set; }

    public bool ShowEmailToPublic { get; set; }

    // ============================================================
    // STATUS
    // ============================================================

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    // ============================================================
    // LOCATION
    // ============================================================

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public int? AreaID { get; set; }

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    // ============================================================
    // PROPERTY DETAILS
    // ============================================================

    public string? PlotNumber { get; set; }

    public decimal? LandArea { get; set; }

    public decimal? BuiltUpArea { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? PropertyAge { get; set; }

    // ============================================================
    // FEATURED / PUBLICATION
    // ============================================================

    public bool IsFeatured { get; set; }

    public DateTime? FeaturedUntil { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    // ============================================================
    // AUDIT
    // ============================================================

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    // ============================================================
    // MEDIA
    // ============================================================

    public List<AdvertisementImageResponse> Images { get; set; } = new();

    public List<AdvertisementVideoResponse> Videos { get; set; } = new();

    public List<AdvertisementDocumentResponse> Documents { get; set; } = new();

    // ============================================================
    // APPROVAL HISTORY
    // ============================================================

    public List<ApprovalHistoryResponse> ApprovalHistory { get; set; } = new();
}