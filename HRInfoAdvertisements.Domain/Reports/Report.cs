namespace HRInfoAdvertisements.Domain.Entities;

public class Report
{
    public long ReportID { get; set; }

    public long AdvertisementID { get; set; }

    public long ReportedByUserID { get; set; }

    public int ReportReasonID { get; set; }

    public string? Comments { get; set; }

    public string Status { get; set; } = "Open";

    public long? ReviewedByUserID { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public User ReportedByUser { get; set; } = null!;

    public User? ReviewedByUser { get; set; }

    public ReportReason ReportReason { get; set; } = null!;
}