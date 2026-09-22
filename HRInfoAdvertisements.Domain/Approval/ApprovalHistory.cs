namespace HRInfoAdvertisements.Domain.Entities;

public class ApprovalHistory
{
    public long ApprovalHistoryID { get; set; }

    public long ApprovalRequestID { get; set; }

    public long ActionedByUserID { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Comments { get; set; }

    public DateTime ActionDate { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;

    public User ActionedByUser { get; set; } = null!;
}