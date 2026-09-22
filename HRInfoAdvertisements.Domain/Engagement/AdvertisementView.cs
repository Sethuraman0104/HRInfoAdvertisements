namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementView
{
    public long AdvertisementViewID { get; set; }

    public long AdvertisementID { get; set; }

    public long? UserID { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime ViewedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public User? User { get; set; }
}