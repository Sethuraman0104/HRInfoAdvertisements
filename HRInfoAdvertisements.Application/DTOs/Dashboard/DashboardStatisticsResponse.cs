namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class DashboardStatisticsResponse
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string GroupBy { get; set; } = string.Empty;

    public List<DashboardStatisticsItemResponse> Items { get; set; } = new();
}