using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Security
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

    // Advertisements
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<AdvertisementCategory> AdvertisementCategories => Set<AdvertisementCategory>();
    public DbSet<AdvertisementType> AdvertisementTypes => Set<AdvertisementType>();
    public DbSet<AdvertisementStatus> AdvertisementStatuses => Set<AdvertisementStatus>();
    public DbSet<AdvertisementFeature> AdvertisementFeatures => Set<AdvertisementFeature>();
    public DbSet<AdvertisementFeatureValue> AdvertisementFeatureValues => Set<AdvertisementFeatureValue>();
    public DbSet<AdvertisementImage> AdvertisementImages => Set<AdvertisementImage>();
    public DbSet<AdvertisementVideo> AdvertisementVideos => Set<AdvertisementVideo>();
    public DbSet<AdvertisementDocument> AdvertisementDocuments => Set<AdvertisementDocument>();

    // Locations
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Area> Areas => Set<Area>();

    // Engagement
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<AdvertisementView> AdvertisementViews => Set<AdvertisementView>();
    public DbSet<AdvertisementEnquiry> AdvertisementEnquiries => Set<AdvertisementEnquiry>();
    public DbSet<AdvertisementEnquiryMessage> AdvertisementEnquiryMessages
    => Set<AdvertisementEnquiryMessage>();
    public DbSet<Message> Messages => Set<Message>();

    // Approval
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();
    public DbSet<RejectionReason> RejectionReasons => Set<RejectionReason>();

    // Reports
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportReason> ReportReasons => Set<ReportReason>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    // Authentication
    public DbSet<OTPRequest> OTPRequests => Set<OTPRequest>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<AdvertisementFavorite>
    AdvertisementFavorites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}