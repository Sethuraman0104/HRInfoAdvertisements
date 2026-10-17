
using HRInfoAdvertisements.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class UserPackageEntitlementConfiguration
    : IEntityTypeConfiguration<UserPackageEntitlement>
{
    public void Configure(EntityTypeBuilder<UserPackageEntitlement> builder)
    {
        builder.ToTable(
            "UserPackageEntitlements",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_UserPackageEntitlements_DateRange",
                    "[EndDate] IS NULL OR [EndDate] > [StartDate]");

                table.HasCheckConstraint(
                    "CK_UserPackageEntitlements_Status",
                    "[Status] IN ('PENDING', 'ACTIVE', 'CANCELLED', 'EXPIRED')");

                table.HasCheckConstraint(
                    "CK_UserPackageEntitlements_NonNegativeLimits",
                    "[MonthlyAdvertisementPostLimit] >= 0 AND " +
                    "[MonthlySenderEnquiryLimit] >= 0 AND " +
                    "[MonthlyAdvertisementEnquiryLimit] >= 0 AND " +
                    "[MonthlyReplyLimit] >= 0");
            });

        builder.HasKey(x => x.UserPackageEntitlementID);

        builder.Property(x => x.MonthlyAdvertisementPostLimit)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MonthlySenderEnquiryLimit)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MonthlyAdvertisementEnquiryLimit)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MonthlyReplyLimit)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserID,
            x.Status,
            x.StartDate,
            x.EndDate
        });

        // Only one entitlement marked ACTIVE per user.
        builder.HasIndex(x => x.UserID)
            .IsUnique()
            .HasFilter("[Status] = 'ACTIVE'");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EnquiryPackage)
            .WithMany(x => x.Entitlements)
            .HasForeignKey(x => x.EnquiryPackageID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
