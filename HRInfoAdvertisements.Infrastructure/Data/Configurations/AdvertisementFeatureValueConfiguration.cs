using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementFeatureValueConfiguration
    : IEntityTypeConfiguration<AdvertisementFeatureValue>
{
    public void Configure(EntityTypeBuilder<AdvertisementFeatureValue> builder)
    {
        builder.ToTable("AdvertisementFeatureValues");

        builder.HasKey(x => x.AdvertisementFeatureValueID);

        builder.Property(x => x.FeatureValue)
            .HasMaxLength(1000);

        builder.Property(x => x.FeatureValueAr)
            .HasMaxLength(1000);

        builder.HasIndex(x => new
        {
            x.AdvertisementID,
            x.AdvertisementFeatureID
        }).IsUnique();

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.FeatureValues)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Feature)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.AdvertisementFeatureID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}