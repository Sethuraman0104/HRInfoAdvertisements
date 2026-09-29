using System.Net.Http.Headers;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.Favorite;

namespace HRInfoAdvertisements.Admin.Services;

public class PublicFavoritesApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly PublicAuthenticationStateProvider
        _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };


    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public PublicFavoritesApiService(
        IHttpClientFactory httpClientFactory,
        PublicAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory =
            httpClientFactory;

        _authenticationStateProvider =
            authenticationStateProvider;
    }


    // ============================================================
    // HTTP CLIENT
    //
    // PublicFavoritesApiService uses the public API client.
    // The JWT is attached directly from the public authentication
    // state provider when the user is signed in.
    // ============================================================

    private HttpClient CreateAuthenticatedClient()
    {
        var client =
            _httpClientFactory
                .CreateClient(
                    "HRInfoAdvertisementsPublicAPI");

        var accessToken =
            _authenticationStateProvider.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "The user is not authenticated or the access token is unavailable.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return client;
    }


    // ============================================================
    // GET MY FAVORITES
    //
    // GET:
    // api/v1/favorites
    //
    // Used by the homepage to determine which advertisement cards
    // should display the filled favorite heart.
    // ============================================================

    public async Task<List<FavoriteResponse>>
        GetMyFavoritesAsync()
    {
        var client =
            CreateAuthenticatedClient();

        var response =
            await client.GetAsync(
                "api/v1/favorites");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve favorites. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new List<FavoriteResponse>();
        }

        return JsonSerializer.Deserialize<
                   List<FavoriteResponse>>(
                   responseBody,
                   JsonOptions)
               ?? new List<FavoriteResponse>();
    }


    // ============================================================
    // ADD FAVORITE
    //
    // POST:
    // api/v1/favorites/{advertisementId}
    // ============================================================

    public async Task AddFavoriteAsync(
        long advertisementId)
    {
        var client =
            CreateAuthenticatedClient();

        var response =
            await client.PostAsync(
                $"api/v1/favorites/{advertisementId}",
                content: null);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to add advertisement to favorites. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Response: {responseBody}");
        }
    }


    // ============================================================
    // REMOVE FAVORITE
    //
    // DELETE:
    // api/v1/favorites/{advertisementId}
    // ============================================================

    public async Task RemoveFavoriteAsync(
        long advertisementId)
    {
        var client =
            CreateAuthenticatedClient();

        var response =
            await client.DeleteAsync(
                $"api/v1/favorites/{advertisementId}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to remove advertisement from favorites. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Response: {responseBody}");
        }
    }
}