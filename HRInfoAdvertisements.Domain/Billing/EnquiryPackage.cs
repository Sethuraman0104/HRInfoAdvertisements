namespace HRInfoAdvertisements.Domain.Billing;

public class EnquiryPackage
{
    public int EnquiryPackageID { get; set; }

    // Stable identifier used by business rules, independent of display name.
    public string PackageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Maximum new enquiries a subscriber may send per UTC calendar month.
    public int MonthlySenderEnquiryLimit { get; set; }

    // Maximum new enquiries an advertisement may receive per UTC calendar month.
    public int MonthlyAdvertisementEnquiryLimit { get; set; }

    // Maximum replies a user may send across all enquiries per UTC calendar month.
public int MonthlyReplyLimit { get; set; }

// Maximum new advertisement submissions per UTC calendar month.
public int MonthlyAdvertisementPostLimit { get; set; }

    public int? ValidityDays { get; set; }

    public decimal Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public ICollection<UserPackageEntitlement> Entitlements { get; set; }
        = new List<UserPackageEntitlement>();
}

