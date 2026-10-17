using HRInfoAdvertisements.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class PackageEntitlementContributionConfiguration
    : IEntityTypeConfiguration<PackageEntitlementContribution>
{
    public void Configure(EntityTypeBuilder<PackageEntitlementContribution> builder)
    {
        builder.ToTable("PackageEntitlementContributions", table =>
        {
            table.HasCheckConstraint(
                "CK_PackageEntitlementContributions_Limits",
                "[MonthlyAdvertisementPostLimit] >= 0 AND " +
                "[MonthlySenderEnquiryLimit] >= 0 AND " +
                "[MonthlyAdvertisementEnquiryLimit] >= 0 AND " +
                "[MonthlyReplyLimit] >= 0");

            table.HasCheckConstraint(
                "CK_PackageEntitlementContributions_Validity",
                "[ValidityDays] > 0 AND [ContributionEndDate] > [ContributionStartDate]");
        });

        builder.HasKey(x => x.PackageEntitlementContributionID);

        builder.Property(x => x.PackageCode)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.PackageName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.PaymentTransactionID)
            .IsUnique();

        builder.HasIndex(x => x.UserPackageEntitlementID)
            .IsUnique()
            .HasFilter("[IsLegacyBaseline] = 1");

        builder.HasIndex(x => new
        {
            x.UserPackageEntitlementID,
            x.ContributionStartDate,
            x.ContributionEndDate
        });

        builder.HasOne(x => x.UserPackageEntitlement)
            .WithMany()
            .HasForeignKey(x => x.UserPackageEntitlementID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentTransaction)
            .WithOne()
            .HasForeignKey<PackageEntitlementContribution>(x => x.PaymentTransactionID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


