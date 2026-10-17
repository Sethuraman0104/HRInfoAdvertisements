
using HRInfoAdvertisements.Domain.Entities;

namespace HRInfoAdvertisements.Domain.Billing;

public static class UserPackageEntitlementStatuses
{
    public const string Pending = "PENDING";
    public const string Active = "ACTIVE";
    public const string Cancelled = "CANCELLED";
    public const string Expired = "EXPIRED";
}

public class UserPackageEntitlement
{
    public long UserPackageEntitlementID { get; set; }

    public long UserID { get; set; }

    public int EnquiryPackageID { get; set; }

    // Limits are snapshotted when the paid entitlement is activated.
    // Later package configuration changes must not alter this entitlement.
    public int MonthlyAdvertisementPostLimit { get; set; }

    public int MonthlySenderEnquiryLimit { get; set; }

    public int MonthlyAdvertisementEnquiryLimit { get; set; }

    public int MonthlyReplyLimit { get; set; }

    // UTC timestamps: StartDate inclusive, EndDate exclusive.
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } =
        UserPackageEntitlementStatuses.Pending;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;

    public EnquiryPackage EnquiryPackage { get; set; } = null!;

    public ICollection<PaymentTransaction> Payments { get; set; }
        = new List<PaymentTransaction>();
}
