namespace HRInfoAdvertisements.Domain.Entities;

public class OTPRequest
{
    public long OTPRequestID { get; set; }

    public long? UserID { get; set; }

    public string Destination { get; set; } = string.Empty;

    public string OTPHash { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; } = 5;

    public bool IsConsumed { get; set; }

    public DateTime? ConsumedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public User? User { get; set; }
}