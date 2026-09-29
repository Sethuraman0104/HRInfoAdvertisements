namespace HRInfoAdvertisements.Application.DTOs.Favorite;

public class FavoriteAdminListResponse
{
    // ============================================================
    // SUMMARY
    // ============================================================

    public int TotalFavorites { get; set; }

    public int UniqueUsers { get; set; }

    public int UniqueAdvertisements { get; set; }

    // ============================================================
    // PAGINATION
    // ============================================================

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    // ============================================================
    // DATA
    // ============================================================

    public List<FavoriteAdminListItemResponse> Items { get; set; }
        = new();
}