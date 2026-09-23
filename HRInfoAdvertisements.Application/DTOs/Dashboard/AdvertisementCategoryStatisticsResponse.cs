namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class AdvertisementCategoryStatisticsResponse
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? CategoryNameAr { get; set; }

    public int Count { get; set; }
}