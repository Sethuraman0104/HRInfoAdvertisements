using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementVideoConfiguration
    : IEntityTypeConfiguration<AdvertisementVideo>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementVideo> builder)
    {
        builder.ToTable("AdvertisementVideos");

        builder.HasKey(x =>
            x.AdvertisementVideoID);

        builder.Property(x =>
                x.AdvertisementVideoID)
            .HasColumnName("AdvertisementVideoID")
            .ValueGeneratedOnAdd();

        builder.Property(x =>
                x.AdvertisementID)
            .HasColumnName("AdvertisementID")
            .IsRequired();

        builder.Property(x =>
                x.FileName)
            .HasColumnName("FileName")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x =>
                x.FileURL)
            .HasColumnName("FileURL")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x =>
                x.StorageKey)
            .HasColumnName("StorageKey")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x =>
                x.ContentType)
            .HasColumnName("ContentType")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x =>
                x.FileSize)
            .HasColumnName("FileSize")
            .IsRequired(false);

        builder.Property(x =>
                x.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .IsRequired();

        builder.Property(x =>
                x.CreatedDate)
            .HasColumnName("CreatedDate")
            .IsRequired();

        builder.HasOne(x =>
                x.Advertisement)
            .WithMany(x =>
                x.Videos)
            .HasForeignKey(x =>
                x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}