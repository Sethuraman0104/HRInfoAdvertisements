using HRInfoAdvertisements.Application.DTOs.Authentication;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request);

    Task<AuthResponse> LoginAsync(
        LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request);

    Task<bool> LogoutAsync(
        long userId,
        string refreshToken);

    Task<bool> ForgotPasswordAsync(
        ForgotPasswordRequest request);

    Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request);

    Task<bool> ChangePasswordAsync(
        long userId,
        ChangePasswordRequest request);

    Task<AuthResponse> GetCurrentUserAsync(
        long userId);

    Task<AuthResponse> VerifyEmailOtpAsync(
        VerifyEmailOtpRequest request);

    Task<bool> ResendEmailVerificationAsync(
        ResendEmailVerificationRequest request);
}