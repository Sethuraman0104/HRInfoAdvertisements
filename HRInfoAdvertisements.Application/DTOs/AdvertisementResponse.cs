using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

namespace HRInfoAdvertisements.Application.DTOs.Advertisements;

public class AdvertisementResponse
{
    public long AdvertisementID { get; set; }

    public long UserID { get; set; }

    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int AdvertisementTypeID { get; set; }

    public string AdvertisementTypeName { get; set; } = string.Empty;

    public int StatusID { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string AdvertisementNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public int? AreaID { get; set; }

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? PlotNumber { get; set; }

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

    // ============================================================
    // MEDIA
    // ============================================================

    public List<AdvertisementImageResponse> Images { get; set; }
        = new();
}