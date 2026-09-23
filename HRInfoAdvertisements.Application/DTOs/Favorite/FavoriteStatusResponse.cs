namespace HRInfoAdvertisements.Application.DTOs.Favorite;

public class FavoriteStatusResponse
{
    public long AdvertisementID { get; set; }

    public bool IsFavorite { get; set; }

    public DateTime? FavoritedDate { get; set; }
}