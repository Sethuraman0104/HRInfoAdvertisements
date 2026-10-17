using HRInfoAdvertisements.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class EnquiryPackageConfiguration
    : IEntityTypeConfiguration<EnquiryPackage>
{
    
public void Configure(EntityTypeBuilder<EnquiryPackage> builder)
{
    builder.ToTable("EnquiryPackages", table =>
    {
        table.HasCheckConstraint(
            "CK_EnquiryPackages_ReplyLimit",
            "[MonthlyReplyLimit] >= 0");

        table.HasCheckConstraint(
            "CK_EnquiryPackages_AdvertisementPostLimit",
            "[MonthlyAdvertisementPostLimit] >= 0");

        table.HasCheckConstraint(
            "CK_EnquiryPackages_Price",
            "[Price] >= 0");

        table.HasCheckConstraint(
            "CK_EnquiryPackages_SenderLimit",
            "[MonthlySenderEnquiryLimit] >= 0");

        table.HasCheckConstraint(
            "CK_EnquiryPackages_AdvertisementLimit",
            "[MonthlyAdvertisementEnquiryLimit] >= 0");

        table.HasCheckConstraint(
            "CK_EnquiryPackages_ValidityDays",
            "[ValidityDays] IS NULL OR [ValidityDays] > 0");
    });

    builder.HasKey(x => x.EnquiryPackageID);

    builder.Property(x => x.PackageCode)
        .HasMaxLength(50)
        .IsUnicode(false)
        .IsRequired();

    builder.HasIndex(x => x.PackageCode)
        .IsUnique();

    builder.Property(x => x.Name)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.Description)
        .HasMaxLength(1000);

    builder.Property(x => x.Price)
        .HasPrecision(18, 3);

    builder.Property(x => x.CurrencyCode)
        .HasMaxLength(3)
        .IsUnicode(false)
        .IsRequired();

    builder.Property(x => x.ValidityDays)
        .IsRequired(false);

    builder.Property(x => x.IsActive)
        .IsRequired();

    builder.HasIndex(x => x.Name)
        .IsUnique();

    builder.Property(x => x.MonthlyAdvertisementPostLimit)
        .IsRequired();
}
}