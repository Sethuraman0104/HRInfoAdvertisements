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

    // Store UTC timestamps. The entitlement is valid from StartDate inclusive
    // until EndDate exclusive; a null EndDate means no scheduled end date.
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
