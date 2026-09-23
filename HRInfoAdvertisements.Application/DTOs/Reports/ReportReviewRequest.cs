namespace HRInfoAdvertisements.Application.DTOs.Reports;

public class ReportReviewRequest
{
    public string Status { get; set; } = string.Empty;

    public string? Comments { get; set; }
}