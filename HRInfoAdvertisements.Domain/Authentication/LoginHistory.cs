namespace HRInfoAdvertisements.Domain.Entities;

public class LoginHistory
{
    public long LoginHistoryID { get; set; }

    public long? UserID { get; set; }

    public string LoginIdentifier { get; set; } = string.Empty;

    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime LoginDate { get; set; }

    public User? User { get; set; }
}