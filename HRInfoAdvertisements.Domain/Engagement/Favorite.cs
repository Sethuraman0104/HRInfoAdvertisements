namespace HRInfoAdvertisements.Domain.Entities;

public class Favorite
{
    public long FavoriteID { get; set; }

    public long UserID { get; set; }

    public long AdvertisementID { get; set; }

    public DateTime CreatedDate { get; set; }

    public User User { get; set; } = null!;

    public Advertisement Advertisement { get; set; } = null!;
}