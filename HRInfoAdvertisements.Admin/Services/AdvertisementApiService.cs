using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.DTOs.Common;
using System.Text.Json;

namespace HRInfoAdvertisements.Admin.Services;

public class AdvertisementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;


    public AdvertisementApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    // ============================================================
    // HTTP CLIENT - ADMIN
    // ============================================================

    private HttpClient CreateClient()
    {
        return _httpClientFactory
            .CreateClient("HRInfoAdvertisementsAPI");
    }


    // ============================================================
    // HTTP CLIENT - PUBLIC AUTHENTICATED USER
    // ============================================================

    private HttpClient CreatePublicClient()
    {
        return _httpClientFactory
            .CreateClient("HRInfoAdvertisementsPublicAPI");
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
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        if (maxPrice.HasValue)
        {
            query.Add(
                $"maxPrice={Uri.EscapeDataString(
                    maxPrice.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        var url =
            "api/v1/advertisements?" +
            string.Join("&", query);


        var client = CreateClient();


        return await client
            .GetFromJsonAsync<AdvertisementListResponse>(
                url);
    }


    // ============================================================
    // SEARCH ADVERTISEMENTS
    // ============================================================

    public async Task<PagedResponse<AdvertisementSearchResponse>?>
        SearchAsync(
            AdvertisementSearchRequest request)
    {
        var query = new List<string>
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
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        if (request.MaxPrice.HasValue)
        {
            query.Add(
                $"MaxPrice={Uri.EscapeDataString(
                    request.MaxPrice.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))}");
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
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        if (request.MaxLandArea.HasValue)
        {
            query.Add(
                $"MaxLandArea={Uri.EscapeDataString(
                    request.MaxLandArea.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        if (request.MinBuiltUpArea.HasValue)
        {
            query.Add(
                $"MinBuiltUpArea={Uri.EscapeDataString(
                    request.MinBuiltUpArea.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))}");
        }


        if (request.MaxBuiltUpArea.HasValue)
        {
            query.Add(
                $"MaxBuiltUpArea={Uri.EscapeDataString(
                    request.MaxBuiltUpArea.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))}");
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


        var client = CreateClient();


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


        var client = CreateClient();


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
        var client = CreateClient();


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
        var client = CreateClient();


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
        var client = CreateClient();


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


        var client = CreateClient();


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


        var parameters = new List<string>();


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


        var client = CreateClient();


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


        var parameters = new List<string>();


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


        var client = CreateClient();


        return await client
            .GetFromJsonAsync<List<LookupItemResponse>>(
                url)
            ?? new List<LookupItemResponse>();
    }


    // ============================================================
    // CREATE ADVERTISEMENT
    // ============================================================

    // ============================================================
// CREATE ADVERTISEMENT
// ============================================================

public async Task<AdvertisementResponse?> CreateAdvertisementAsync(
    CreateAdvertisementRequest request)
{
    var client = CreatePublicClient();

    var response = await client.PostAsJsonAsync(
        "api/v1/advertisements",
        request);

    var responseBody =
        await response.Content.ReadAsStringAsync();

    // TEMPORARY DIAGNOSTIC
    Console.WriteLine(
        $"CREATE ADVERTISEMENT STATUS: {(int)response.StatusCode} {response.StatusCode}");

    Console.WriteLine(
        $"CREATE ADVERTISEMENT RESPONSE: {responseBody}");

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Create advertisement failed. " +
            $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
            $"Response: {responseBody}");
    }

    if (string.IsNullOrWhiteSpace(responseBody))
    {
        throw new InvalidOperationException(
            "The API returned a successful response, but no advertisement data was returned.");
    }

    var result =
        JsonSerializer.Deserialize<AdvertisementResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

    if (result is null)
    {
        throw new InvalidOperationException(
            $"The API returned an invalid advertisement response: {responseBody}");
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

        var client = CreatePublicClient();

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/advertisements/{advertisementId}",
                request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<AdvertisementResponse>();
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

        var client = CreatePublicClient();

        var response =
            await client.PostAsync(
                $"api/v1/advertisements/{advertisementId}/submit",
                null);

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

        var client = CreatePublicClient();

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
                isPrimary ? "true" : "false"),
            "isPrimary");

        var response =
            await client.PostAsync(
                $"api/v1/advertisements/{advertisementId}/media/images",
                content);

        return response.IsSuccessStatusCode;
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

        var client = CreateClient();

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

        var client = CreatePublicClient();

        var response =
            await client.DeleteAsync(
                $"api/v1/advertisements/{advertisementId}/media/images/{imageId}");

        return response.IsSuccessStatusCode;
    }


    // ============================================================
    // SET PRIMARY IMAGE
    // ============================================================

    public async Task<bool>
        SetPrimaryAdvertisementImageAsync(
            long advertisementId,
            long imageId)
    {
        if (advertisementId <= 0 ||
            imageId <= 0)
        {
            return false;
        }

        var client = CreatePublicClient();

        var response =
            await client.PostAsync(
                $"api/v1/advertisements/{advertisementId}/media/images/{imageId}/primary",
                null);

        return response.IsSuccessStatusCode;
    }
}
