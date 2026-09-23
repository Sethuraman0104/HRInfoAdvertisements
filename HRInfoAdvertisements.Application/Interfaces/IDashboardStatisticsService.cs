using HRInfoAdvertisements.Application.DTOs.Dashboard;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IDashboardStatisticsService
{
    Task<DashboardStatisticsResponse> GetStatisticsAsync(
        DashboardStatisticsRequest request);
}