namespace HRInfoAdvertisements.Domain.Entities;

public class ReportReason
{
    public int ReportReasonID { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string ReasonText { get; set; } = string.Empty;

    public string? ReasonTextAr { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<Report> Reports { get; set; } = new List<Report>();
}