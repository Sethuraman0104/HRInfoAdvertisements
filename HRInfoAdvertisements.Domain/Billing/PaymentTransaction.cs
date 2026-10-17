namespace HRInfoAdvertisements.Domain.Billing;

public static class PaymentTransactionStatuses
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Failed = "FAILED";
    public const string Refunded = "REFUNDED";
}

public class PaymentTransaction
{
    public long PaymentTransactionID { get; set; }

    public long UserPackageEntitlementID { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentProvider { get; set; } = string.Empty;

    public string? ProviderTransactionReference { get; set; }

    public string? ExternalRefundReference { get; set; }

    public string Status { get; set; } = PaymentTransactionStatuses.Pending;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedDate { get; set; }

    public DateTime? RefundedDate { get; set; }

    public UserPackageEntitlement UserPackageEntitlement { get; set; } = null!;
}
