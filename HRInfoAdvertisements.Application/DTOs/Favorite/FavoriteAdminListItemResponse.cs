namespace HRInfoAdvertisements.Application.DTOs.Favorite;

public class FavoriteAdminListItemResponse
{
    // ============================================================
    // FAVORITE
    // ============================================================

    public long AdvertisementFavoriteID { get; set; }

    public DateTime FavoritedDate { get; set; }

    // ============================================================
    // USER
    // ============================================================

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? MobileNo { get; set; }

    // ============================================================
    // ADVERTISEMENT
    // ============================================================

    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } =
        string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    // ============================================================
    // CLASSIFICATION
    // ============================================================

    public string? CategoryName { get; set; }

    public string? AdvertisementTypeName { get; set; }

    // ============================================================
    // ADVERTISEMENT STATUS
    // ============================================================

    public string? StatusCode { get; set; }

    public string? StatusName { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }
}