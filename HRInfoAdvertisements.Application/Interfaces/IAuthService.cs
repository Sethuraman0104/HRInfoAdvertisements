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

    Task<AuthResponse> GetCurrentUserAsync(
        long userId);
}