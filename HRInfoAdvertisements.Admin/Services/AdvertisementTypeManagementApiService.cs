using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

namespace HRInfoAdvertisements.Admin.Services;

public class AdvertisementTypeManagementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public AdvertisementTypeManagementApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
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

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedAccessException(
                "Administrator access token is not available. " +
                "Please sign in again.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return client;
    }

    // ============================================================
    // GET ADVERTISEMENT TYPES
    // ============================================================

    public async Task<AdvertisementTypeListResponse>
        GetAdvertisementTypesAsync(
            string? search = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 20)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var query = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(
                    search.Trim())}");
        }

        if (isActive.HasValue)
        {
            query.Add(
                $"isActive={isActive.Value
                    .ToString()
                    .ToLowerInvariant()}");
        }

        var url =
            "api/v1/advertisement-types?" +
            string.Join("&", query);

        var client = CreateClient();

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve advertisement types. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new AdvertisementTypeListResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        return JsonSerializer.Deserialize<
                   AdvertisementTypeListResponse>(
                       responseBody,
                       JsonOptions)
               ?? new AdvertisementTypeListResponse
               {
                   PageNumber = pageNumber,
                   PageSize = pageSize
               };
    }

    // ============================================================
    // GET BY ID
    // ============================================================

    public async Task<AdvertisementTypeDetailResponse?>
        GetAdvertisementTypeByIdAsync(
            int advertisementTypeId)
    {
        if (advertisementTypeId <= 0)
        {
            return null;
        }

        var client = CreateClient();

        var response =
            await client.GetAsync(
                $"api/v1/advertisement-types/" +
                $"{advertisementTypeId}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve advertisement type. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        return JsonSerializer.Deserialize<
            AdvertisementTypeDetailResponse>(
                responseBody,
                JsonOptions);
    }

    // ============================================================
    // CREATE
    // ============================================================

    public async Task<int?>
        CreateAdvertisementTypeAsync(
            CreateAdvertisementTypeRequest request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "api/v1/advertisement-types",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to create advertisement type. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        var result =
            JsonSerializer.Deserialize<
                AdvertisementTypeDetailResponse>(
                    responseBody,
                    JsonOptions);

        return result?.AdvertisementTypeID;
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public async Task<bool>
        UpdateAdvertisementTypeAsync(
            int advertisementTypeId,
            UpdateAdvertisementTypeRequest request)
    {
        if (advertisementTypeId <= 0)
        {
            return false;
        }

        var client = CreateClient();

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/advertisement-types/" +
                $"{advertisementTypeId}",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update advertisement type. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }

    // ============================================================
    // UPDATE STATUS
    // ============================================================

    public async Task<bool>
        UpdateAdvertisementTypeStatusAsync(
            int advertisementTypeId,
            bool isActive)
    {
        if (advertisementTypeId <= 0)
        {
            return false;
        }

        var client = CreateClient();

        var request =
            new UpdateAdvertisementTypeStatusRequest
            {
                IsActive = isActive
            };

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/advertisement-types/" +
                $"{advertisementTypeId}/status",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update advertisement type status. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }
}