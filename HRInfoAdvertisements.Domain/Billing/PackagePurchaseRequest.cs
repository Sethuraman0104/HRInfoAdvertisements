using HRInfoAdvertisements.Domain.Entities;

namespace HRInfoAdvertisements.Domain.Billing;

public static class PackagePurchaseRequestStatuses
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Completed = "COMPLETED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
}

public class PackagePurchaseRequest
{
    public long PackagePurchaseRequestID { get; set; }

    public long UserID { get; set; }

    public int EnquiryPackageID { get; set; }

    // Snapshot of the package price when the request was created.
    public decimal Amount { get; set; }

    // Snapshot of the package currency when the request was created.
    public string CurrencyCode { get; set; } = string.Empty;

    public string Status { get; set; } =
        PackagePurchaseRequestStatuses.Pending;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;

    public EnquiryPackage EnquiryPackage { get; set; } = null!;
}

