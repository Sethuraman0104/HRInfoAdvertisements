namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class EnquiryPackageResponse
{
    public int EnquiryPackageID { get; init; }

    public string PackageCode { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int MonthlySenderEnquiryLimit { get; init; }

    public int MonthlyAdvertisementEnquiryLimit { get; init; }

    public int? ValidityDays { get; init; }

    public decimal Price { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;
}
