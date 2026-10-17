namespace HRInfoAdvertisements.Domain.Billing;

public class PackageEntitlementContribution
{
    public long PackageEntitlementContributionID { get; set; }

    public long UserPackageEntitlementID { get; set; }

    public long? PaymentTransactionID { get; set; }

    public bool IsLegacyBaseline { get; set; }

    public long EnquiryPackageID { get; set; }

    public string PackageCode { get; set; } = string.Empty;

    public string PackageName { get; set; } = string.Empty;

    public int MonthlyAdvertisementPostLimit { get; set; }

    public int MonthlySenderEnquiryLimit { get; set; }

    public int MonthlyAdvertisementEnquiryLimit { get; set; }

    public int MonthlyReplyLimit { get; set; }

    public int ValidityDays { get; set; }

    public DateTime ContributionStartDate { get; set; }

    public DateTime ContributionEndDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public UserPackageEntitlement UserPackageEntitlement { get; set; } = null!;

    public PaymentTransaction? PaymentTransaction { get; set; }
}

