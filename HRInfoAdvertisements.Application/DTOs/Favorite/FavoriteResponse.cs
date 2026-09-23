namespace HRInfoAdvertisements.Application.DTOs.Favorite;

public class FavoriteResponse
{
    public long AdvertisementFavoriteID { get; set; }

    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } =
        string.Empty;

    public string? Title { get; set; }

    public string? TitleAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } =
        "BHD";

    public string? CategoryName { get; set; }

    public string? AdvertisementTypeName { get; set; }

    public string? StatusCode { get; set; }

    public string? StatusName { get; set; }

    public string? AddressLine { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime FavoritedDate { get; set; }
}