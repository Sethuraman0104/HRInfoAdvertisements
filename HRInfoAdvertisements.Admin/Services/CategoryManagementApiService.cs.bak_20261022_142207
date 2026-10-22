using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Admin.Services;
using HRInfoAdvertisements.Application.DTOs.CategoryManagement;

namespace HRInfoAdvertisements.Admin.Services;

public class CategoryManagementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public CategoryManagementApiService(
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
    // GET CATEGORIES
    // ============================================================

    public async Task<CategoryListResponse>
        GetCategoriesAsync(
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
                $"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (isActive.HasValue)
        {
            query.Add(
                $"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var url =
            "api/v1/categories?" +
            string.Join("&", query);

        var client = CreateClient();

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve categories. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new CategoryListResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        return JsonSerializer.Deserialize<CategoryListResponse>(
                   responseBody,
                   JsonOptions)
               ?? new CategoryListResponse
               {
                   PageNumber = pageNumber,
                   PageSize = pageSize
               };
    }

    // ============================================================
    // GET CATEGORY BY ID
    // ============================================================

    public async Task<CategoryDetailResponse?>
        GetCategoryByIdAsync(
            int categoryId)
    {
        if (categoryId <= 0)
        {
            return null;
        }

        var client = CreateClient();

        var response =
            await client.GetAsync(
                $"api/v1/categories/{categoryId}");

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
                $"Unable to retrieve category. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CategoryDetailResponse>(
            responseBody,
            JsonOptions);
    }

    // ============================================================
    // CREATE CATEGORY
    // ============================================================

    public async Task<int?>
        CreateCategoryAsync(
            CreateCategoryRequest request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "api/v1/categories",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to create category. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        var result =
            JsonSerializer.Deserialize<CategoryDetailResponse>(
                responseBody,
                JsonOptions);

        return result?.CategoryID;
    }

    // ============================================================
    // UPDATE CATEGORY
    // ============================================================

    public async Task<bool>
        UpdateCategoryAsync(
            int categoryId,
            UpdateCategoryRequest request)
    {
        if (categoryId <= 0)
        {
            return false;
        }

        var client = CreateClient();

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/categories/{categoryId}",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update category. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }

    // ============================================================
    // UPDATE CATEGORY STATUS
    // ============================================================

    public async Task<bool>
        UpdateCategoryStatusAsync(
            int categoryId,
            bool isActive)
    {
        if (categoryId <= 0)
        {
            return false;
        }

        var client = CreateClient();

        var request = new UpdateCategoryStatusRequest
        {
            IsActive = isActive
        };

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/categories/{categoryId}/status",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update category status. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }
}