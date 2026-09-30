namespace HRInfoAdvertisements.Application.DTOs.Authentication;

public class VerifyEmailOtpRequest
{
    public string Email { get; set; } = string.Empty;

    public string OTP { get; set; } = string.Empty;
}