using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class ReportSubmissionService : IReportSubmissionService
{
    private readonly ApplicationDbContext _context;

    public ReportSubmissionService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<long?> CreateReportAsync(
        long userId,
        CreateReportRequest request)
    {
        var advertisement = await _context.Advertisements
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.AdvertisementID == request.AdvertisementID);

        if (advertisement == null)
        {
            return null;
        }

        var reportReasonExists = await _context.ReportReasons
            .AnyAsync(x =>
                x.ReportReasonID == request.ReportReasonID &&
                x.IsActive);

        if (!reportReasonExists)
        {
            return null;
        }

        var userExists = await _context.Users
            .AnyAsync(x =>
                x.UserID == userId &&
                x.AccountStatus == "Active");

        if (!userExists)
        {
            return null;
        }

        // Prevent the same user from reporting the same
        // advertisement multiple times while an existing
        // report is still active.
        var existingReport = await _context.Reports
            .AnyAsync(x =>
                x.AdvertisementID == request.AdvertisementID &&
                x.ReportedByUserID == userId &&
                (x.Status == "Open" ||
                 x.Status == "Reviewed"));

        if (existingReport)
        {
            return null;
        }

        var report = new Report
        {
            AdvertisementID = request.AdvertisementID,
            ReportedByUserID = userId,
            ReportReasonID = request.ReportReasonID,
            Comments = string.IsNullOrWhiteSpace(request.Comments)
                ? null
                : request.Comments.Trim(),
            Status = "Open",
            CreatedDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);

        await _context.SaveChangesAsync();

        return report.ReportID;
    }
}