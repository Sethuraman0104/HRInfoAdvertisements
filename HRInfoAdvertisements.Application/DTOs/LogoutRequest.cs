namespace HRInfoAdvertisements.Application.DTOs.Authentication;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}