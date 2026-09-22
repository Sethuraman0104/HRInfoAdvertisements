namespace HRInfoAdvertisements.Domain.Entities;

public class RefreshToken
{
    public long RefreshTokenID { get; set; }

    public long UserID { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIPAddress { get; set; }

    public string? RevokedByIPAddress { get; set; }

    public DateTime CreatedDate { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive =>
        RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}