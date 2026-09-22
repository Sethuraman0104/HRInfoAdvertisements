using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementImageConfiguration
    : IEntityTypeConfiguration<AdvertisementImage>
{
    public void Configure(EntityTypeBuilder<AdvertisementImage> builder)
    {
        builder.ToTable("AdvertisementImages");

        builder.HasKey(x => x.AdvertisementImageID);

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FileURL)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(1000);

        builder.Property(x => x.ContentType)
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.AdvertisementID,
            x.DisplayOrder
        });

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}