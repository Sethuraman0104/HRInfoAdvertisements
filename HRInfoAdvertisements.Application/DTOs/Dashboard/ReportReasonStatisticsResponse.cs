namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class ReportReasonStatisticsResponse
{
    public int ReportReasonID { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string ReasonText { get; set; } = string.Empty;

    public string? ReasonTextAr { get; set; }

    public int Count { get; set; }
}