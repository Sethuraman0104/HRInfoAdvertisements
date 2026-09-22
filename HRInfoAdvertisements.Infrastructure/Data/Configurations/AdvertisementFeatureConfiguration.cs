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

        builder.Property(x => x.FeatureName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.FeatureNameAr)
            .HasMaxLength(150);

        builder.Property(x => x.DataType)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}