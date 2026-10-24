
using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementFeatureConfiguration
    : IEntityTypeConfiguration<AdvertisementFeature>
{
    public void Configure(EntityTypeBuilder<AdvertisementFeature> builder)
    {
        builder.ToTable("AdvertisementFeatures");

        builder.HasKey(x => x.AdvertisementFeatureID);

        builder.Property(x => x.AdvertisementFeatureID)
            .HasColumnName("FeatureID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryID)
            .IsRequired(false);

        builder.Property(x => x.FeatureName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.FeatureNameAr)
            .HasMaxLength(150);

        builder.Property(x => x.HelpText).HasMaxLength(500);
        builder.Property(x => x.HelpTextAr).HasMaxLength(500);
        builder.Property(x => x.ValidationPattern).HasMaxLength(1000);

        builder.Property(x => x.MinValue).HasPrecision(18, 4);
        builder.Property(x => x.MaxValue).HasPrecision(18, 4);

        builder.Property(x => x.DataType)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
