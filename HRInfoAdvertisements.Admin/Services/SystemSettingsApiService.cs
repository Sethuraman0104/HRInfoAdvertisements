using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HRInfoAdvertisements.Application.DTOs.SystemSettings;

namespace HRInfoAdvertisements.Admin.Services;

public class SystemSettingsApiService
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    private const string BaseEndpoint = "api/v1/system-settings";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdminAuthenticationStateProvider _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public SystemSettingsApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }


    // ============================================================
    // HTTP CLIENT
    // ============================================================

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("HRInfoAdvertisementsAPI");

        var accessToken = _authenticationStateProvider.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedAccessException(
                "The administrator is not authenticated.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }


    // ============================================================
    // GET - LIST SYSTEM SETTINGS
    // ============================================================

    public async Task<SystemSettingListResponse> GetSystemSettingsAsync(
        SystemSettingListRequest request)
    {
        var client = CreateClient();

        var queryParameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            queryParameters.Add(
                $"search={Uri.EscapeDataString(request.Search.Trim())}");
        }

        if (request.IsActive.HasValue)
        {
            queryParameters.Add(
                $"isActive={request.IsActive.Value.ToString().ToLowerInvariant()}");
        }

        queryParameters.Add(
            $"pageNumber={request.PageNumber}");

        queryParameters.Add(
            $"pageSize={request.PageSize}");

        var endpoint = BaseEndpoint;

        if (queryParameters.Count > 0)
        {
            endpoint += "?" + string.Join("&", queryParameters);
        }

        var response = await client.GetAsync(endpoint);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to load system settings. " +
                $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"{responseBody}");
        }

        var result = JsonSerializer.Deserialize<SystemSettingListResponse>(
            responseBody,
            JsonOptions);

        return result ?? new SystemSettingListResponse();
    }


    // ============================================================
    // GET - SYSTEM SETTING BY ID
    // ============================================================

    public async Task<SystemSettingDetailResponse?> GetSystemSettingByIdAsync(
        int systemSettingId)
    {
        var client = CreateClient();

        var response = await client.GetAsync(
            $"{BaseEndpoint}/{systemSettingId}");

        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to load system setting. " +
                $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"{responseBody}");
        }

        return JsonSerializer.Deserialize<SystemSettingDetailResponse>(
            responseBody,
            JsonOptions);
    }


    // ============================================================
    // POST - CREATE SYSTEM SETTING
    // ============================================================

    public async Task<SystemSettingDetailResponse?> CreateSystemSettingAsync(
        CreateSystemSettingRequest request)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            BaseEndpoint,
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to create system setting. " +
                $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"{responseBody}");
        }

        return JsonSerializer.Deserialize<SystemSettingDetailResponse>(
            responseBody,
            JsonOptions);
    }


    // ============================================================
    // PUT - UPDATE SYSTEM SETTING
    // ============================================================

    public async Task<SystemSettingDetailResponse?> UpdateSystemSettingAsync(
        int systemSettingId,
        UpdateSystemSettingRequest request)
    {
        var client = CreateClient();

        var response = await client.PutAsJsonAsync(
            $"{BaseEndpoint}/{systemSettingId}",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update system setting. " +
                $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"{responseBody}");
        }

        return JsonSerializer.Deserialize<SystemSettingDetailResponse>(
            responseBody,
            JsonOptions);
    }


    // ============================================================
    // PUT - UPDATE SYSTEM SETTING STATUS
    // ============================================================

    public async Task<bool> UpdateSystemSettingStatusAsync(
        int systemSettingId,
        bool isActive)
    {
        var client = CreateClient();

        var request = new UpdateSystemSettingStatusRequest
        {
            IsActive = isActive
        };

        var response = await client.PutAsJsonAsync(
            $"{BaseEndpoint}/{systemSettingId}/status",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update system setting status. " +
                $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"{responseBody}");
        }

        return true;
    }
}
