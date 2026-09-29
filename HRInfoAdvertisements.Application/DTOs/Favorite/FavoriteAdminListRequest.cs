namespace HRInfoAdvertisements.Application.DTOs.Favorite;

public class FavoriteAdminListRequest
{
    // ============================================================
    // SEARCH / FILTER
    // ============================================================

    public string? Search { get; set; }

    public long? UserID { get; set; }

    public long? AdvertisementID { get; set; }

    // ============================================================
    // PAGINATION
    // ============================================================

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}