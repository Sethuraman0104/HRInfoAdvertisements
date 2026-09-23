namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    // Users
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int LockedUsers { get; set; }

    // Advertisements
    public int TotalAdvertisements { get; set; }
    public int PublishedAdvertisements { get; set; }
    public int PendingAdvertisements { get; set; }
    public int RejectedAdvertisements { get; set; }
    public int ExpiredAdvertisements { get; set; }

    // Reports
    public int TotalReports { get; set; }
    public int OpenReports { get; set; }
    public int ReviewedReports { get; set; }
    public int ResolvedReports { get; set; }
    public int RejectedReports { get; set; }

    // Engagement
    public int TotalFavorites { get; set; }
    public int TotalEnquiries { get; set; }
    public int TotalMessages { get; set; }

    // Audit
    public int TotalAuditLogs { get; set; }
}