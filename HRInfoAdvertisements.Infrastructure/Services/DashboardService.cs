using HRInfoAdvertisements.Application.DTOs.Dashboard;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var result = new DashboardSummaryResponse
        {
            // Users
            TotalUsers = await _context.Users.CountAsync(),

            ActiveUsers = await _context.Users
                .CountAsync(x => x.AccountStatus == "Active"),

            SuspendedUsers = await _context.Users
                .CountAsync(x => x.AccountStatus == "Suspended"),

            LockedUsers = await _context.Users
                .CountAsync(x =>
                    x.LockoutEndDate.HasValue &&
                    x.LockoutEndDate.Value > DateTime.UtcNow),

            // Advertisements
            TotalAdvertisements = await _context.Advertisements.CountAsync(),

            PublishedAdvertisements = await _context.Advertisements
                .CountAsync(x => x.StatusID == 4),

            PendingAdvertisements = await _context.Advertisements
                .CountAsync(x => x.StatusID == 2),

            RejectedAdvertisements = await _context.Advertisements
                .CountAsync(x => x.StatusID == 3),

            ExpiredAdvertisements = await _context.Advertisements
                .CountAsync(x => x.StatusID == 6),

            // Reports
            TotalReports = await _context.Reports.CountAsync(),

            OpenReports = await _context.Reports
                .CountAsync(x => x.Status == "Open"),

            ReviewedReports = await _context.Reports
                .CountAsync(x => x.Status == "Reviewed"),

            ResolvedReports = await _context.Reports
                .CountAsync(x => x.Status == "Resolved"),

            RejectedReports = await _context.Reports
                .CountAsync(x => x.Status == "Rejected"),

            // Engagement
            TotalFavorites = await _context.Favorites.CountAsync(),

            TotalEnquiries = await _context.AdvertisementEnquiries.CountAsync(),

            TotalMessages = await _context.Messages.CountAsync(),

            // Audit
            TotalAuditLogs = await _context.AuditLogs.CountAsync()
        };

        return result;
    }
    public async Task<List<AdvertisementStatusStatisticsResponse>>
    GetAdvertisementsByStatusAsync()
{
    return await _context.AdvertisementStatuses
        .AsNoTracking()
        .OrderBy(x => x.DisplayOrder)
        .Select(x => new AdvertisementStatusStatisticsResponse
        {
            StatusID = x.StatusID,
            StatusCode = x.StatusCode,
            StatusName = x.StatusName,
            StatusNameAr = x.StatusNameAr,
            Count = x.Advertisements.Count()
        })
        .ToListAsync();
}
public async Task<List<AdvertisementCategoryStatisticsResponse>>
    GetAdvertisementsByCategoryAsync()
{
    return await _context.AdvertisementCategories
        .AsNoTracking()
        .OrderBy(x => x.DisplayOrder)
        .Select(x => new AdvertisementCategoryStatisticsResponse
        {
            CategoryID = x.CategoryID,
            CategoryName = x.CategoryName,
            CategoryNameAr = x.CategoryNameAr,
            Count = x.Advertisements.Count()
        })
        .ToListAsync();
}

public async Task<List<ReportReasonStatisticsResponse>>
    GetReportsByReasonAsync()
{
    return await _context.ReportReasons
        .AsNoTracking()
        .OrderBy(x => x.DisplayOrder)
        .Select(x => new ReportReasonStatisticsResponse
        {
            ReportReasonID = x.ReportReasonID,
            ReasonCode = x.ReasonCode,
            ReasonText = x.ReasonText,
            ReasonTextAr = x.ReasonTextAr,
            Count = x.Reports.Count()
        })
        .ToListAsync();
}
}