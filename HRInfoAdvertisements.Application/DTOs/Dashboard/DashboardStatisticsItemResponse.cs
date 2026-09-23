namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class DashboardStatisticsItemResponse
{
    public string Period { get; set; } = string.Empty;

    // Users
    public int NewUsers { get; set; }

    // Advertisements
    public int NewAdvertisements { get; set; }
    public int PublishedAdvertisements { get; set; }

    // Engagement
    public int AdvertisementViews { get; set; }
    public int Favorites { get; set; }
    public int Enquiries { get; set; }
    public int Messages { get; set; }

    // Reports
    public int Reports { get; set; }
}