using HRInfoAdvertisements.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration
    : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable(
            "PaymentTransactions",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_PaymentTransactions_Amount",
                    "[Amount] >= 0");

                table.HasCheckConstraint(
                    "CK_PaymentTransactions_Status",
                    "[Status] IN ('PENDING', 'CONFIRMED', 'FAILED', 'REFUNDED')");
            });

        builder.HasKey(x => x.PaymentTransactionID);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 3);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentProvider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderTransactionReference)
            .HasMaxLength(200);

        builder.Property(x => x.ExternalRefundReference)
            .HasMaxLength(200);

        builder.HasIndex(x => x.ExternalRefundReference)
            .IsUnique()
            .HasFilter("[ExternalRefundReference] IS NOT NULL")
            .HasDatabaseName("IX_PaymentTransactions_ExternalRefundReference");

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => x.UserPackageEntitlementID);

        builder.HasIndex(x => new
        {
            x.PaymentProvider,
            x.ProviderTransactionReference
        })
        .IsUnique()
        .HasFilter("[ProviderTransactionReference] IS NOT NULL");

        builder.HasOne(x => x.UserPackageEntitlement)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.UserPackageEntitlementID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
