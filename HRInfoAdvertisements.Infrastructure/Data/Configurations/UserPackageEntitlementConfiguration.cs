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
            });

        builder.HasKey(x => x.UserPackageEntitlementID);

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

        // Prevent more than one entitlement marked ACTIVE for a user.
        // The service must expire/cancel the previous entitlement before
        // activating a replacement.
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
