namespace HRInfoAdvertisements.Domain.Entities;

public class UserSession
{
    public long UserSessionID { get; set; }

    public long UserID { get; set; }

    public string SessionTokenHash { get; set; } = string.Empty;

    public string? DeviceName { get; set; }

    public string? DeviceType { get; set; }

    public string? IPAddress { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastActivityDate { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? RevokedDate { get; set; }

    public User User { get; set; } = null!;
}