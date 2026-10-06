using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminProfileApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    // ============================================================
    // API RESPONSE WRAPPER
    // ============================================================

    private sealed class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public AdminProfileApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory =
            httpClientFactory;

        _authenticationStateProvider =
            authenticationStateProvider;
    }

    // ============================================================
    // HTTP CLIENT
    // ============================================================

    private HttpClient CreateClient()
    {
        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var accessToken =
            _authenticationStateProvider.AccessToken;

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN PROFILE HTTP CLIENT");

        Console.WriteLine(
            $"API BASE ADDRESS: {client.BaseAddress}");

        Console.WriteLine(
            $"ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(accessToken)}");

        Console.WriteLine(
            $"ACCESS TOKEN LENGTH: " +
            $"{accessToken?.Length ?? 0}");

        Console.WriteLine(
            $"TOKEN EXPIRES AT: " +
            $"{_authenticationStateProvider.ExpiresAt}");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Console.WriteLine(
                "AUTHORIZATION HEADER: NO TOKEN AVAILABLE.");

            Console.WriteLine(
                "================================================");

            throw new UnauthorizedAccessException(
                "Administrator access token is not available. " +
                "Please sign in again.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        Console.WriteLine(
            "AUTHORIZATION HEADER: Bearer token attached.");

        Console.WriteLine(
            "================================================");

        return client;
    }

    // ============================================================
    // GET MY PROFILE
    // ============================================================

    public async Task<AdminProfileResponse?>
        GetMyProfileAsync()
    {
        var client =
            CreateClient();

        const string url =
            "api/v1/admin/profile";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN PROFILE - GET PROFILE");

        Console.WriteLine(
            $"GET: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"PROFILE STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"PROFILE RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        // TEMPORARY DEBUG
        Console.WriteLine(
            "ADMIN PROFILE RAW RESPONSE:");

        Console.WriteLine(
            responseBody);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                "ADMIN PROFILE GET FAILED");

            Console.WriteLine(
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode})");

            Console.WriteLine(
                $"Response: {responseBody}");

            Console.WriteLine(
                "================================================");

            throw new HttpRequestException(
                $"Unable to load administrator profile. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        var apiResponse =
            JsonSerializer.Deserialize<
                ApiResponse<AdminProfileResponse>>(
                    responseBody,
                    JsonOptions);

        if (apiResponse == null)
        {
            return null;
        }

        if (!apiResponse.Success)
        {
            throw new InvalidOperationException(
                apiResponse.Message ??
                "Unable to load administrator profile.");
        }

        return apiResponse.Data;
    }

    // ============================================================
    // UPDATE MY PROFILE
    // ============================================================

    public async Task<AdminProfileResponse?>
        UpdateMyProfileAsync(
            UpdateAdminProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var client =
            CreateClient();

        const string url =
            "api/v1/admin/profile";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN PROFILE - UPDATE PROFILE");

        Console.WriteLine(
            $"PUT: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PutAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"UPDATE PROFILE STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"UPDATE PROFILE RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        // TEMPORARY DEBUG
        Console.WriteLine(
            "UPDATE PROFILE RAW RESPONSE:");

        Console.WriteLine(
            responseBody);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                "ADMIN PROFILE UPDATE FAILED");

            Console.WriteLine(
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode})");

            Console.WriteLine(
                $"Response: {responseBody}");

            Console.WriteLine(
                "================================================");

            throw new HttpRequestException(
                $"Unable to update administrator profile. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        var apiResponse =
            JsonSerializer.Deserialize<
                ApiResponse<AdminProfileResponse>>(
                    responseBody,
                    JsonOptions);

        if (apiResponse == null)
        {
            return null;
        }

        if (!apiResponse.Success)
        {
            throw new InvalidOperationException(
                apiResponse.Message ??
                "Unable to update administrator profile.");
        }

        return apiResponse.Data;
    }

    // ============================================================
    // REVOKE SESSION
    // ============================================================

    public async Task RevokeSessionAsync(
        long sessionId)
    {
        if (sessionId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sessionId));
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/profile/sessions/{sessionId}";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN PROFILE - REVOKE SESSION");

        Console.WriteLine(
            $"DELETE: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.DeleteAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"REVOKE SESSION STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                "ADMIN PROFILE REVOKE SESSION FAILED");

            Console.WriteLine(
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode})");

            Console.WriteLine(
                $"Response: {responseBody}");

            Console.WriteLine(
                "================================================");

            throw new HttpRequestException(
                $"Unable to revoke administrator session. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        Console.WriteLine(
            "ADMIN PROFILE SESSION REVOKED");

        Console.WriteLine(
            "================================================");
    }
}