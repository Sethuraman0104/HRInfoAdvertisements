using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.DTOs.Common;

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
    // HTTP CLIENT
    // ============================================================

    private HttpClient CreateClient()
    {
        return _httpClientFactory
            .CreateClient("HRInfoAdvertisementsAPI");
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
}