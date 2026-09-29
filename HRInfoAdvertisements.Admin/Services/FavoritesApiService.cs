using System.Net.Http.Headers;
using System.Text.Json;
using HRInfoAdvertisements.Admin.Services;
using HRInfoAdvertisements.Application.DTOs.Favorite;

namespace HRInfoAdvertisements.Admin.Services;

public class FavoritesApiService
{
    // ============================================================
    // FIELDS
    // ============================================================

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public FavoritesApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider
            authenticationStateProvider)
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

        var token =
            _authenticationStateProvider.AccessToken;

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        return client;
    }

    // ============================================================
    // GET ADMIN FAVORITES
    // ============================================================

    public async Task<FavoriteAdminListResponse>
        GetFavoritesAsync(
            FavoriteAdminListRequest request)
    {
        var client = CreateClient();

        var query =
            new List<string>();

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(request.Search.Trim())}");
        }

        // --------------------------------------------------------
        // User ID
        // --------------------------------------------------------

        if (request.UserID.HasValue)
        {
            query.Add(
                $"userID={request.UserID.Value}");
        }

        // --------------------------------------------------------
        // Advertisement ID
        // --------------------------------------------------------

        if (request.AdvertisementID.HasValue)
        {
            query.Add(
                $"advertisementID={request.AdvertisementID.Value}");
        }

        // --------------------------------------------------------
        // Pagination
        // --------------------------------------------------------

        query.Add(
            $"pageNumber={request.PageNumber}");

        query.Add(
            $"pageSize={request.PageSize}");

        var queryString =
            string.Join("&", query);

        var url =
            $"api/v1/favorites/admin?{queryString}";

        var response =
            await client.GetAsync(url);

        var body =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to load favorites. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {body}");
        }

        var result =
            JsonSerializer.Deserialize<
                FavoriteAdminListResponse>(
                    body,
                    _jsonOptions);

        if (result == null)
        {
            throw new InvalidOperationException(
                "The favorites API returned an empty response.");
        }

        return result;
    }
}