namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class ApprovalHistoryResponse
{
    public long ApprovalHistoryID { get; set; }

    public long AdvertisementID { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Comments { get; set; }

    public long ActionByUserID { get; set; }

    public string? ActionByUserName { get; set; }

    public DateTime ActionDate { get; set; }

    public long? RejectionReasonID { get; set; }

    public string? RejectionReason { get; set; }
}