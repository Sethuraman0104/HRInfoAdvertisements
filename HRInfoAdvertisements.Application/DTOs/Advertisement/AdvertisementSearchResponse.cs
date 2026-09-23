namespace HRInfoAdvertisements.Application.DTOs.Advertisement;

public class AdvertisementSearchResponse
{
    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public long UserID { get; set; }

    public string? UserName { get; set; }

    public string? Title { get; set; }

    public string? TitleAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    public int? CategoryID { get; set; }

    public string? CategoryName { get; set; }

    public int? AdvertisementTypeID { get; set; }

    public string? AdvertisementTypeName { get; set; }

    public int? StatusID { get; set; }

    public string? StatusCode { get; set; }

    public string? StatusName { get; set; }

    public long? CountryID { get; set; }

    public long? StateID { get; set; }

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

    public DateTime CreatedDate { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }
}