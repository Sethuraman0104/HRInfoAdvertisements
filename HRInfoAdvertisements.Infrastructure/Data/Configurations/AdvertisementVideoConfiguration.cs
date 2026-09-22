using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementVideoConfiguration
    : IEntityTypeConfiguration<AdvertisementVideo>
{
    public void Configure(EntityTypeBuilder<AdvertisementVideo> builder)
    {
        builder.ToTable("AdvertisementVideos");

        builder.HasKey(x => x.AdvertisementVideoID);

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

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Videos)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}