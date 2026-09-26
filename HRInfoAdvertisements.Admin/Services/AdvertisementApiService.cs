using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.DTOs.Common;

namespace HRInfoAdvertisements.Admin.Services;

public class AdvertisementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly PublicAuthenticationStateProvider
        _authenticationStateProvider;


    public AdvertisementApiService(
        IHttpClientFactory httpClientFactory,
        PublicAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory =
            httpClientFactory;

        _authenticationStateProvider =
            authenticationStateProvider;
    }


    // ============================================================
    // HTTP CLIENT - ADMIN
    // ============================================================

    private HttpClient CreateClient()
    {
        return _httpClientFactory
            .CreateClient(
                "HRInfoAdvertisementsAPI");
    }


    // ============================================================
    // HTTP CLIENT - PUBLIC
    // ============================================================

    private HttpClient CreatePublicClient()
    {
        return _httpClientFactory
            .CreateClient(
                "HRInfoAdvertisementsPublicAPI");
    }


    // ============================================================
    // HTTP CLIENT - PUBLIC AUTHENTICATED USER
    //
    // IMPORTANT:
    // Do NOT use PublicJwtHandler here.
    //
    // The PublicAuthenticationStateProvider used by the Blazor
    // circuit contains the actual access token.
    // We attach that token directly to this HttpClient.
    // ============================================================

    private HttpClient CreateAuthenticatedPublicClient()
    {
        var client =
            CreatePublicClient();

        var accessToken =
            _authenticationStateProvider.AccessToken;


        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADVERTISEMENT SERVICE AUTHENTICATION CHECK");

        Console.WriteLine(
            $"ADVERTISEMENT SERVICE TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(accessToken)}");

        Console.WriteLine(
            $"ADVERTISEMENT SERVICE TOKEN LENGTH: " +
            $"{accessToken?.Length ?? 0}");

        Console.WriteLine(
            $"ADVERTISEMENT SERVICE IS AUTHENTICATED: " +
            $"{_authenticationStateProvider.IsAuthenticated}");

        Console.WriteLine(
            "================================================");


        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "The user is not authenticated or the access token is unavailable.");
        }


        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);


        Console.WriteLine(
            "ADVERTISEMENT SERVICE JWT ATTACHED.");


        return client;
    }


    // ============================================================
    // GET PUBLISHED ADVERTISEMENTS
    // ============================================================

    public async Task<AdvertisementListResponse?>
        GetPublishedAdvertisementsAsync(
            int pageNumber = 1,
            int pageSize = 12,
            string? search = null,
            int? categoryId = null,
            int? typeId = null,
            int? countryId = null,
            int? stateId = null,
            int? cityId = null,
            int? areaId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
    {
        var query =
            new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };


        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(search.Trim())}");
        }


        if (categoryId.HasValue)
        {
            query.Add(
                $"categoryId={categoryId.Value}");
        }


        if (typeId.HasValue)
        {
            query.Add(
                $"typeId={typeId.Value}");
        }


        if (countryId.HasValue)
        {
            query.Add(
                $"countryId={countryId.Value}");
        }


        if (stateId.HasValue)
        {
            query.Add(
                $"stateId={stateId.Value}");
        }


        if (cityId.HasValue)
        {
            query.Add(
                $"cityId={cityId.Value}");
        }


        if (areaId.HasValue)
        {
            query.Add(
                $"areaId={areaId.Value}");
        }


        if (minPrice.HasValue)
        {
            query.Add(
                $"minPrice={Uri.EscapeDataString(
                    minPrice.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (maxPrice.HasValue)
        {
            query.Add(
                $"maxPrice={Uri.EscapeDataString(
                    maxPrice.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        var url =
            "api/v1/advertisements?" +
            string.Join("&", query);


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<AdvertisementListResponse>(
                url);
    }


    // ============================================================
    // SEARCH ADVERTISEMENTS
    // ============================================================

    public async Task<
        PagedResponse<AdvertisementSearchResponse>?>
        SearchAsync(
            AdvertisementSearchRequest request)
    {
        var query =
            new List<string>
            {
                "StatusCode=PUBLISHED",
                $"PageNumber={request.PageNumber}",
                $"PageSize={request.PageSize}"
            };


        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query.Add(
                $"Keyword={Uri.EscapeDataString(
                    request.Keyword.Trim())}");
        }


        if (request.CategoryID.HasValue)
        {
            query.Add(
                $"CategoryID={request.CategoryID.Value}");
        }


        if (request.AdvertisementTypeID.HasValue)
        {
            query.Add(
                $"AdvertisementTypeID={request.AdvertisementTypeID.Value}");
        }


        if (request.CountryID.HasValue)
        {
            query.Add(
                $"CountryID={request.CountryID.Value}");
        }


        if (request.StateID.HasValue)
        {
            query.Add(
                $"StateID={request.StateID.Value}");
        }


        if (request.MinPrice.HasValue)
        {
            query.Add(
                $"MinPrice={Uri.EscapeDataString(
                    request.MinPrice.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.MaxPrice.HasValue)
        {
            query.Add(
                $"MaxPrice={Uri.EscapeDataString(
                    request.MaxPrice.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.MinBedrooms.HasValue)
        {
            query.Add(
                $"MinBedrooms={request.MinBedrooms.Value}");
        }


        if (request.MaxBedrooms.HasValue)
        {
            query.Add(
                $"MaxBedrooms={request.MaxBedrooms.Value}");
        }


        if (request.MinBathrooms.HasValue)
        {
            query.Add(
                $"MinBathrooms={request.MinBathrooms.Value}");
        }


        if (request.MaxBathrooms.HasValue)
        {
            query.Add(
                $"MaxBathrooms={request.MaxBathrooms.Value}");
        }


        if (request.MinLandArea.HasValue)
        {
            query.Add(
                $"MinLandArea={Uri.EscapeDataString(
                    request.MinLandArea.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.MaxLandArea.HasValue)
        {
            query.Add(
                $"MaxLandArea={Uri.EscapeDataString(
                    request.MaxLandArea.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.MinBuiltUpArea.HasValue)
        {
            query.Add(
                $"MinBuiltUpArea={Uri.EscapeDataString(
                    request.MinBuiltUpArea.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.MaxBuiltUpArea.HasValue)
        {
            query.Add(
                $"MaxBuiltUpArea={Uri.EscapeDataString(
                    request.MaxBuiltUpArea.Value.ToString(
                        CultureInfo.InvariantCulture))}");
        }


        if (request.IsNegotiable.HasValue)
        {
            query.Add(
                $"IsNegotiable={request.IsNegotiable.Value}");
        }


        if (request.IsFeatured.HasValue)
        {
            query.Add(
                $"IsFeatured={request.IsFeatured.Value}");
        }


        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query.Add(
                $"SortBy={Uri.EscapeDataString(
                    request.SortBy.Trim())}");
        }


        if (!string.IsNullOrWhiteSpace(request.SortDirection))
        {
            query.Add(
                $"SortDirection={Uri.EscapeDataString(
                    request.SortDirection.Trim())}");
        }


        var url =
            "api/v1/advertisements/search?" +
            string.Join("&", query);


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<
                PagedResponse<AdvertisementSearchResponse>>(
                url);
    }


    // ============================================================
    // GET ADVERTISEMENT BY ID
    // ============================================================

    public async Task<AdvertisementResponse?>
        GetByIdAsync(
            long advertisementId)
    {
        if (advertisementId <= 0)
        {
            return null;
        }


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<AdvertisementResponse>(
                $"api/v1/advertisements/{advertisementId}");
    }


    // ============================================================
    // GET ADVERTISEMENT CATEGORIES
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetCategoriesAsync()
    {
        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                "api/v1/advertisement-lookups/categories")
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // GET ADVERTISEMENT TYPES
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetAdvertisementTypesAsync()
    {
        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                "api/v1/advertisement-lookups/types")
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // GET COUNTRIES
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetCountriesAsync()
    {
        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                "api/v1/advertisement-lookups/countries")
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // GET STATES
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetStatesAsync(
            int countryId)
    {
        if (countryId <= 0)
        {
            return new List<LookupItemResponse>();
        }


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                $"api/v1/advertisement-lookups/states/{countryId}")
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // GET CITIES
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetCitiesAsync(
            int countryId,
            int? stateId = null)
    {
        if (countryId <= 0)
        {
            return new List<LookupItemResponse>();
        }


        var parameters =
            new List<string>();


        if (stateId.HasValue &&
            stateId.Value > 0)
        {
            parameters.Add(
                $"stateId={stateId.Value}");
        }


        var url =
            $"api/v1/advertisement-lookups/cities/{countryId}";


        if (parameters.Count > 0)
        {
            url +=
                "?" +
                string.Join("&", parameters);
        }


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                url)
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // GET AREAS
    // ============================================================

    public async Task<List<LookupItemResponse>>
        GetAreasAsync(
            int countryId,
            int? stateId = null,
            int? cityId = null)
    {
        if (countryId <= 0)
        {
            return new List<LookupItemResponse>();
        }


        var parameters =
            new List<string>();


        if (stateId.HasValue &&
            stateId.Value > 0)
        {
            parameters.Add(
                $"stateId={stateId.Value}");
        }


        if (cityId.HasValue &&
            cityId.Value > 0)
        {
            parameters.Add(
                $"cityId={cityId.Value}");
        }


        var url =
            $"api/v1/advertisement-lookups/areas/{countryId}";


        if (parameters.Count > 0)
        {
            url +=
                "?" +
                string.Join("&", parameters);
        }


        var client =
            CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                url)
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // CREATE ADVERTISEMENT
    // ============================================================

    public async Task<AdvertisementResponse?>
        CreateAdvertisementAsync(
            CreateAdvertisementRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }


        var client =
            CreateAuthenticatedPublicClient();


        var response =
            await client.PostAsJsonAsync(
                "api/v1/advertisements",
                request);


        var responseBody =
            await response.Content.ReadAsStringAsync();


        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"CREATE ADVERTISEMENT STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"CREATE ADVERTISEMENT RESPONSE: " +
            $"{responseBody}");

        Console.WriteLine(
            "================================================");


        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Create advertisement failed. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }


        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new InvalidOperationException(
                "The API returned a successful response, " +
                "but no advertisement data was returned.");
        }


        var result =
            JsonSerializer.Deserialize<
                AdvertisementResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        if (result is null)
        {
            throw new InvalidOperationException(
                "The API returned an invalid advertisement response: " +
                responseBody);
        }


        return result;
    }


    // ============================================================
    // UPDATE ADVERTISEMENT
    // ============================================================

    public async Task<AdvertisementResponse?>
        UpdateAdvertisementAsync(
            long advertisementId,
            UpdateAdvertisementRequest request)
    {
        if (advertisementId <= 0)
        {
            return null;
        }


        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }


        var client =
            CreateAuthenticatedPublicClient();


        var response =
            await client.PutAsJsonAsync(
                $"api/v1/advertisements/{advertisementId}",
                request);


        var responseBody =
            await response.Content.ReadAsStringAsync();


        Console.WriteLine(
            $"UPDATE ADVERTISEMENT STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"UPDATE ADVERTISEMENT RESPONSE: " +
            $"{responseBody}");


        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Update advertisement failed. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }


        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }


        return JsonSerializer.Deserialize<
            AdvertisementResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }


    // ============================================================
    // SUBMIT ADVERTISEMENT FOR APPROVAL
    // ============================================================

    public async Task<bool>
        SubmitAdvertisementAsync(
            long advertisementId)
    {
        if (advertisementId <= 0)
        {
            return false;
        }


        var client =
            CreateAuthenticatedPublicClient();


        var response =
            await client.PostAsync(
                $"api/v1/advertisements/{advertisementId}/submit",
                null);


        Console.WriteLine(
            $"SUBMIT ADVERTISEMENT STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");


        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"SUBMIT ADVERTISEMENT RESPONSE: " +
                $"{responseBody}");
        }


        return response.IsSuccessStatusCode;
    }

    // ============================================================
    // UPLOAD ADVERTISEMENT IMAGE
    // ============================================================

    public async Task<bool>
    UploadAdvertisementImageAsync(
        long advertisementId,
        Stream fileStream,
        string fileName,
        string contentType,
        bool isPrimary = false)
{
    if (advertisementId <= 0)
    {
        return false;
    }

    if (fileStream is null)
    {
        throw new ArgumentNullException(
            nameof(fileStream));
    }

    var client =
        CreateAuthenticatedPublicClient();


    using var content =
        new MultipartFormDataContent();


    using var streamContent =
        new StreamContent(fileStream);


    if (!string.IsNullOrWhiteSpace(contentType))
    {
        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                contentType);
    }


    content.Add(
        streamContent,
        "file",
        fileName);


    content.Add(
        new StringContent(
            isPrimary
                ? "true"
                : "false"),
        "isPrimary");


    Console.WriteLine(
        "================================================");

    Console.WriteLine(
        "UPLOADING ADVERTISEMENT IMAGE");

    Console.WriteLine(
        $"ADVERTISEMENT ID: {advertisementId}");

    Console.WriteLine(
        $"FILE NAME: {fileName}");

    Console.WriteLine(
        $"CONTENT TYPE: {contentType}");

    Console.WriteLine(
        $"IS PRIMARY: {isPrimary}");

    Console.WriteLine(
        "POST: " +
        $"api/v1/advertisements/{advertisementId}/media/images");

    Console.WriteLine(
        "================================================");


    var response =
        await client.PostAsync(
            $"api/v1/advertisements/{advertisementId}/media/images",
            content);


    var responseBody =
        await response.Content.ReadAsStringAsync();


    Console.WriteLine(
        "================================================");

    Console.WriteLine(
        "ADVERTISEMENT IMAGE UPLOAD RESULT");

    Console.WriteLine(
        $"STATUS: {(int)response.StatusCode} {response.StatusCode}");

    Console.WriteLine(
        $"RESPONSE: {responseBody}");

    Console.WriteLine(
        "================================================");


    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Image upload failed. " +
            $"HTTP {(int)response.StatusCode} " +
            $"({response.StatusCode}). " +
            $"Response: {responseBody}");
    }


    return true;
}

// ============================================================
// GET ADVERTISEMENT BY ID
// ============================================================

// public async Task<AdvertisementResponse?> GetAdvertisementByIdAsync(
//     long advertisementId)
// {
//     if (advertisementId <= 0)
//     {
//         return null;
//     }

//     try
//     {
//         var client =
//             CreateAuthenticatedPublicClient();

//         Console.WriteLine(
//             "================================================");

//         Console.WriteLine(
//             "GET ADVERTISEMENT BY ID");

//         Console.WriteLine(
//             $"ADVERTISEMENT ID: {advertisementId}");

//         Console.WriteLine(
//             "================================================");

//         var response =
//             await client.GetAsync(
//                 $"api/v1/advertisements/{advertisementId}");

//         var responseBody =
//             await response.Content.ReadAsStringAsync();

//         Console.WriteLine(
//             $"GET ADVERTISEMENT RESPONSE: " +
//             $"{(int)response.StatusCode} " +
//             $"{response.StatusCode}");

//         if (!response.IsSuccessStatusCode)
//         {
//             Console.WriteLine(
//                 $"GET ADVERTISEMENT FAILED: " +
//                 $"{responseBody}");

//             return null;
//         }

//         if (string.IsNullOrWhiteSpace(responseBody))
//         {
//             Console.WriteLine(
//                 "GET ADVERTISEMENT: Empty response.");

//             return null;
//         }

//         var advertisement =
//             System.Text.Json.JsonSerializer.Deserialize<
//                 AdvertisementResponse>(
//                     responseBody,
//                     new System.Text.Json.JsonSerializerOptions
//                     {
//                         PropertyNameCaseInsensitive = true
//                     });

//         Console.WriteLine(
//             $"GET ADVERTISEMENT SUCCESS: " +
//             $"{advertisement?.AdvertisementID}");

//         Console.WriteLine(
//             $"GET ADVERTISEMENT IMAGE COUNT: " +
//             $"{advertisement?.Images?.Count ?? 0}");

//         return advertisement;
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine(
//             "================================================");

//         Console.WriteLine(
//             "GET ADVERTISEMENT BY ID ERROR");

//         Console.WriteLine(
//             ex.ToString());

//         Console.WriteLine(
//             "================================================");

//         throw;
//     }
// }
public async Task<AdvertisementResponse?> GetAdvertisementByIdAsync(
    long advertisementId)
{
    if (advertisementId <= 0)
    {
        return null;
    }

    try
    {
        var client =
            CreateAuthenticatedPublicClient();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "GET ADVERTISEMENT BY ID");

        Console.WriteLine(
            $"ADVERTISEMENT ID: {advertisementId}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(
                $"api/v1/advertisements/{advertisementId}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"GET ADVERTISEMENT RESPONSE: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                $"GET ADVERTISEMENT FAILED: {responseBody}");

            return null;
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                "GET ADVERTISEMENT: Empty response.");

            return null;
        }

        var advertisement =
            System.Text.Json.JsonSerializer.Deserialize<
                AdvertisementResponse>(
                    responseBody,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        Console.WriteLine(
            $"GET ADVERTISEMENT SUCCESS: " +
            $"{advertisement?.AdvertisementID}");

        Console.WriteLine(
            $"GET ADVERTISEMENT IMAGE COUNT: " +
            $"{advertisement?.Images?.Count ?? 0}");

        return advertisement;
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "GET ADVERTISEMENT BY ID ERROR");

        Console.WriteLine(
            ex.ToString());

        Console.WriteLine(
            "================================================");

        throw;
    }
}
    // ============================================================
    // GET ADVERTISEMENT IMAGES
    // ============================================================

    public async Task<string?>
        GetAdvertisementImagesAsync(
            long advertisementId)
    {
        if (advertisementId <= 0)
        {
            return null;
        }


        var client =
            CreateClient();


        var response =
            await client.GetAsync(
                $"api/v1/advertisements/{advertisementId}/media/images");


        if (!response.IsSuccessStatusCode)
        {
            return null;
        }


        return await response.Content
            .ReadAsStringAsync();
    }


    // ============================================================
    // DELETE ADVERTISEMENT IMAGE
    // ============================================================

    public async Task<bool>
        DeleteAdvertisementImageAsync(
            long advertisementId,
            long imageId)
    {
        if (advertisementId <= 0 ||
            imageId <= 0)
        {
            return false;
        }


        var client =
            CreateAuthenticatedPublicClient();


        var response =
            await client.DeleteAsync(
                $"api/v1/advertisements/{advertisementId}/media/images/{imageId}");


        Console.WriteLine(
            $"DELETE ADVERTISEMENT IMAGE STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");


        return response.IsSuccessStatusCode;
    }


    // ============================================================
    // SET PRIMARY IMAGE
    // ============================================================

    // ============================================================
// SET PRIMARY ADVERTISEMENT IMAGE
// ============================================================

public async Task<bool>
    SetPrimaryImageAsync(
        long advertisementId,
        long imageId)
{
    if (advertisementId <= 0 ||
        imageId <= 0)
    {
        return false;
    }

    try
    {
        var client =
            CreateAuthenticatedPublicClient();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "SETTING PRIMARY ADVERTISEMENT IMAGE");

        Console.WriteLine(
            $"ADVERTISEMENT ID: {advertisementId}");

        Console.WriteLine(
            $"IMAGE ID: {imageId}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PutAsync(
                $"api/v1/advertisements/" +
                $"{advertisementId}/media/images/" +
                $"{imageId}/primary",
                null);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "SET PRIMARY IMAGE RESULT");

        Console.WriteLine(
            $"STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE: {responseBody}");

        Console.WriteLine(
            "================================================");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                "SET PRIMARY IMAGE FAILED.");

            return false;
        }

        Console.WriteLine(
            "PRIMARY IMAGE UPDATED SUCCESSFULLY.");

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "SET PRIMARY IMAGE ERROR");

        Console.WriteLine(
            ex.ToString());

        Console.WriteLine(
            "================================================");

        throw;
    }
}

// ============================================================
// GET MY ADVERTISEMENTS
// ============================================================

public async Task<List<AdvertisementResponse>>
    GetMyAdvertisementsAsync()
{
    try
    {
        var client =
            CreateAuthenticatedPublicClient();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "GET MY ADVERTISEMENTS");

        Console.WriteLine(
            "GET: api/v1/advertisements/my");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(
                "api/v1/advertisements/my");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "GET MY ADVERTISEMENTS RESULT");

        Console.WriteLine(
            $"STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE LENGTH: " +
            $"{responseBody?.Length ?? 0}");

        Console.WriteLine(
            "================================================");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(
                $"GET MY ADVERTISEMENTS FAILED: " +
                $"{responseBody}");

            return new List<AdvertisementResponse>();
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                "GET MY ADVERTISEMENTS: Empty response.");

            return new List<AdvertisementResponse>();
        }

        var result =
            JsonSerializer.Deserialize<
                AdvertisementListResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result == null)
        {
            Console.WriteLine(
                "GET MY ADVERTISEMENTS: " +
                "Unable to deserialize response.");

            return new List<AdvertisementResponse>();
        }

        Console.WriteLine(
            $"MY ADVERTISEMENTS COUNT: " +
            $"{result.Items?.Count ?? 0}");

        Console.WriteLine(
            $"MY ADVERTISEMENTS TOTAL RECORDS: " +
            $"{result.TotalRecords}");

        Console.WriteLine(
            $"MY ADVERTISEMENTS PAGE: " +
            $"{result.PageNumber} / {result.TotalPages}");

        Console.WriteLine(
            "================================================");

        return result.Items
            ?? new List<AdvertisementResponse>();
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "GET MY ADVERTISEMENTS ERROR");

        Console.WriteLine(
            ex.ToString());

        Console.WriteLine(
            "================================================");

        throw;
    }
}
}