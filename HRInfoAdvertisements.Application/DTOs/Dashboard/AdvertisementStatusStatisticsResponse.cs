namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class AdvertisementStatusStatisticsResponse
{
    public int StatusID { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? StatusNameAr { get; set; }

    public int Count { get; set; }
}