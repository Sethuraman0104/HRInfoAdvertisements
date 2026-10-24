
using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementFeatureOptionConfiguration
    : IEntityTypeConfiguration<AdvertisementFeatureOption>
{
    public void Configure(EntityTypeBuilder<AdvertisementFeatureOption> builder)
    {
        builder.ToTable("AdvertisementFeatureOptions");

        builder.HasKey(x => x.AdvertisementFeatureOptionID);

        builder.Property(x => x.OptionValue)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.OptionValueAr)
            .HasMaxLength(150);

        builder.HasIndex(x => new
        {
            x.AdvertisementFeatureID,
            x.OptionValue
        }).IsUnique();

        builder.HasOne(x => x.Feature)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.AdvertisementFeatureID)
            .HasPrincipalKey(x => x.AdvertisementFeatureID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
