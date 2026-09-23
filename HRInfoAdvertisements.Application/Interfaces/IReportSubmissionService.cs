using HRInfoAdvertisements.Application.DTOs.Reports;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IReportSubmissionService
{
    Task<long?> CreateReportAsync(
        long userId,
        CreateReportRequest request);
}