namespace HRInfoAdvertisements.Application.DTOs.Reports;

public class ReportListItemResponse
{
    public long ReportID { get; set; }

    public long AdvertisementID { get; set; }

    public string? AdvertisementTitle { get; set; }

    public long ReportedByUserID { get; set; }

    public string? ReportedByUserName { get; set; }

    public string? ReportedByEmail { get; set; }

    public int ReportReasonID { get; set; }

    public string? ReportReasonCode { get; set; }

    public string? ReportReasonText { get; set; }

    public string? Comments { get; set; }

    public string Status { get; set; } = string.Empty;

    public long? ReviewedByUserID { get; set; }

    public string? ReviewedByUserName { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }
}