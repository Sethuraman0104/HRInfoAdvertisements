namespace HRInfoAdvertisements.Application.DTOs.Authentication;

public class AuthResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public List<string> Roles { get; set; } = new();

    public List<string> Permissions { get; set; } = new();

    // ============================================================
    // EMAIL VERIFICATION
    // ============================================================

    /// <summary>
    /// Indicates that the user must verify their email address
    /// before authentication can be completed.
    /// </summary>
    public bool RequiresEmailVerification { get; set; }
}