namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class PackagePurchaseRequestResponse
{
    public long PackagePurchaseRequestID { get; init; }

    public int EnquiryPackageID { get; init; }

    public string PackageCode { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedDate { get; init; }

    public DateTime? ModifiedDate { get; init; }
}
