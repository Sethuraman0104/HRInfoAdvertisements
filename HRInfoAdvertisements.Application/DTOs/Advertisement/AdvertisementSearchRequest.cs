namespace HRInfoAdvertisements.Application.DTOs.Advertisement;

public class AdvertisementSearchRequest
{
    public string? Keyword { get; set; }

    public int? CategoryID { get; set; }

    public int? AdvertisementTypeID { get; set; }

    public int? StatusID { get; set; }

    public string? StatusCode { get; set; }

    public long? CountryID { get; set; }

    public long? StateID { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int? MinBedrooms { get; set; }

    public int? MaxBedrooms { get; set; }

    public int? MinBathrooms { get; set; }

    public int? MaxBathrooms { get; set; }

    public decimal? MinLandArea { get; set; }

    public decimal? MaxLandArea { get; set; }

    public decimal? MinBuiltUpArea { get; set; }

    public decimal? MaxBuiltUpArea { get; set; }

    public bool? IsNegotiable { get; set; }

    public bool? IsFeatured { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public DateTime? PublishedFrom { get; set; }

    public DateTime? PublishedTo { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}