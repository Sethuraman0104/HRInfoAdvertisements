using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.Dashboard;

namespace HRInfoAdvertisements.Admin.Services;

public class DashboardApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("HRInfoAdvertisementsAPI");
    }

    public async Task<DashboardSummaryResponse?> GetSummaryAsync()
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<DashboardSummaryResponse>(
            "api/v1/dashboard/summary");
    }

    public async Task<List<AdvertisementStatusStatisticsResponse>>
        GetAdvertisementsByStatusAsync()
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<
            List<AdvertisementStatusStatisticsResponse>>(
                "api/v1/dashboard/advertisements-by-status")
            ?? new List<AdvertisementStatusStatisticsResponse>();
    }

    public async Task<List<AdvertisementCategoryStatisticsResponse>>
        GetAdvertisementsByCategoryAsync()
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<
            List<AdvertisementCategoryStatisticsResponse>>(
                "api/v1/dashboard/advertisements-by-category")
            ?? new List<AdvertisementCategoryStatisticsResponse>();
    }

    public async Task<List<ReportReasonStatisticsResponse>>
        GetReportsByReasonAsync()
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<
            List<ReportReasonStatisticsResponse>>(
                "api/v1/dashboard/reports-by-reason")
            ?? new List<ReportReasonStatisticsResponse>();
    }

    public async Task<DashboardStatisticsResponse?>
        GetStatisticsAsync(DashboardStatisticsRequest request)
    {
        var client = CreateClient();

        var query = new List<string>();

        if (request.FromDate.HasValue)
        {
            query.Add(
                $"FromDate={Uri.EscapeDataString(request.FromDate.Value.ToString("O"))}");
        }

        if (request.ToDate.HasValue)
        {
            query.Add(
                $"ToDate={Uri.EscapeDataString(request.ToDate.Value.ToString("O"))}");
        }

        if (!string.IsNullOrWhiteSpace(request.GroupBy))
        {
            query.Add(
                $"GroupBy={Uri.EscapeDataString(request.GroupBy)}");
        }

        var url = "api/v1/dashboard/statistics";

        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        return await client.GetFromJsonAsync<DashboardStatisticsResponse>(url);
    }
}