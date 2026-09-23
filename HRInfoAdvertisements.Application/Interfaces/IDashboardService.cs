using HRInfoAdvertisements.Application.DTOs.Dashboard;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();

    Task<List<AdvertisementStatusStatisticsResponse>>
        GetAdvertisementsByStatusAsync();

    Task<List<AdvertisementCategoryStatisticsResponse>>
        GetAdvertisementsByCategoryAsync();

    Task<List<ReportReasonStatisticsResponse>>
        GetReportsByReasonAsync();
}