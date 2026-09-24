using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.Authentication;

namespace HRInfoAdvertisements.Admin.Services;

public class AuthApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthResponse?> LoginAsync(
        string email,
        string password)
    {
        var client =
            _httpClientFactory.CreateClient("HRInfoAdvertisementsAPI");

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "api/v1/Auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RefreshTokenAsync(
        string refreshToken)
    {
        var client =
            _httpClientFactory.CreateClient("HRInfoAdvertisementsAPI");

        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var response = await client.PostAsJsonAsync(
            "api/v1/Auth/refresh",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> GetCurrentUserAsync(
    string accessToken)
{
    var client =
        _httpClientFactory.CreateClient(
            "HRInfoAdvertisementsAPI");

    using var request =
        new HttpRequestMessage(
            HttpMethod.Get,
            "api/v1/Auth/me");

    request.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            accessToken);

    var response =
        await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
        return null;
    }

    return await response.Content
        .ReadFromJsonAsync<AuthResponse>();
}

public async Task<bool> LogoutAsync(
    string refreshToken)
{
    var client =
        _httpClientFactory.CreateClient(
            "HRInfoAdvertisementsAPI");

    var request =
        new
        {
            RefreshToken = refreshToken
        };

    var response =
        await client.PostAsJsonAsync(
            "api/v1/Auth/logout",
            request);

    return response.IsSuccessStatusCode;
}
}