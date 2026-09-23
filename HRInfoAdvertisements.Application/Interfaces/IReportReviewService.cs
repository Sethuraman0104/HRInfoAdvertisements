using HRInfoAdvertisements.Application.DTOs.Reports;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IReportReviewService
{
    Task<bool> ReviewReportAsync(
        long reportId,
        ReportReviewRequest request,
        long reviewedByUserId);
}