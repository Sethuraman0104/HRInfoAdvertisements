using System.Text.Json;
using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class ReportReviewService : IReportReviewService
{
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Reviewed",
            "Resolved",
            "Rejected"
        };

    private readonly ApplicationDbContext _context;

    public ReportReviewService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ReviewReportAsync(
        long reportId,
        ReportReviewRequest request,
        long reviewedByUserId)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(x => x.ReportID == reportId);

        if (report == null)
        {
            return false;
        }

        if (!AllowedStatuses.Contains(request.Status))
        {
            return false;
        }

        var reviewerExists = await _context.Users
            .AnyAsync(x =>
                x.UserID == reviewedByUserId &&
                x.AccountStatus == "Active");

        if (!reviewerExists)
        {
            return false;
        }

        var oldValues = new
        {
            report.Status,
            report.ReviewedByUserID,
            report.ReviewedDate,
            report.Comments
        };

        report.Status = request.Status.Trim();
        report.ReviewedByUserID = reviewedByUserId;
        report.ReviewedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Comments))
        {
            report.Comments = request.Comments.Trim();
        }

        var newValues = new
        {
            report.Status,
            report.ReviewedByUserID,
            report.ReviewedDate,
            report.Comments
        };

        var auditLog = new AuditLog
        {
            UserID = reviewedByUserId,
            Action = "REPORT_REVIEW",
            EntityName = "Report",
            EntityID = report.ReportID.ToString(),
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(newValues),
            CreatedDate = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();

        return true;
    }
}