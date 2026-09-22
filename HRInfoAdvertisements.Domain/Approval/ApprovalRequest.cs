namespace HRInfoAdvertisements.Domain.Entities;

public class ApprovalRequest
{
    public long ApprovalRequestID { get; set; }

    public long AdvertisementID { get; set; }

    public long SubmittedByUserID { get; set; }

    public long? AssignedToUserID { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Comments { get; set; }

    public DateTime SubmittedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public User SubmittedByUser { get; set; } = null!;

    public User? AssignedToUser { get; set; }

    public ICollection<ApprovalHistory> ApprovalHistory { get; set; }
        = new List<ApprovalHistory>();
}