using HRInfoAdvertisements.Application.DTOs.Reports;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IReportManagementService
{
    Task<ReportListResponse> GetReportsAsync(
        ReportListRequest request);
}