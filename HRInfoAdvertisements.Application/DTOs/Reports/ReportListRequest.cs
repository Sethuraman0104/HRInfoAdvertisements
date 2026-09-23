namespace HRInfoAdvertisements.Application.DTOs.Reports;

public class ReportListRequest
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public int? ReportReasonID { get; set; }

    public long? ReportedByUserID { get; set; }

    public long? ReviewedByUserID { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}