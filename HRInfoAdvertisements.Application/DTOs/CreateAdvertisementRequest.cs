using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.Advertisements;

public class CreateAdvertisementRequest
{
    [Required]
    public int CategoryID { get; set; }

    [Required]
    public int AdvertisementTypeID { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? TitleAr { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public int? AreaID { get; set; }

    [MaxLength(500)]
    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [MaxLength(100)]
    public string? PlotNumber { get; set; }

    public decimal? LandArea { get; set; }

    public decimal? BuiltUpArea { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? PropertyAge { get; set; }

    public DateTime? ExpiryDate { get; set; }
}