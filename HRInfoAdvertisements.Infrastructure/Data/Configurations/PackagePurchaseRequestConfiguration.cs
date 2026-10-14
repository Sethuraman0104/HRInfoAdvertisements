using HRInfoAdvertisements.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public sealed class PackagePurchaseRequestConfiguration
    : IEntityTypeConfiguration<PackagePurchaseRequest>
{
    public void Configure(
        EntityTypeBuilder<PackagePurchaseRequest> builder)
    {
        builder.ToTable(
            "PackagePurchaseRequests",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_PackagePurchaseRequests_Amount",
                    "[Amount] >= 0");

                table.HasCheckConstraint(
                    "CK_PackagePurchaseRequests_Status",
                    "[Status] IN ('PENDING', 'APPROVED', 'COMPLETED', 'REJECTED', 'CANCELLED')");
            });

        builder.HasKey(x => x.PackagePurchaseRequestID);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => x.UserID);

        // Only one pending purchase request per user.
        builder.HasIndex(x => x.UserID)
    .IsUnique()
    .HasFilter("[Status] IN ('PENDING', 'APPROVED')");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EnquiryPackage)
            .WithMany()
            .HasForeignKey(x => x.EnquiryPackageID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
