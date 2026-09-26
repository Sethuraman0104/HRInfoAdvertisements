using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementImageConfiguration
    : IEntityTypeConfiguration<AdvertisementImage>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementImage> builder)
    {
        builder.ToTable("AdvertisementImages");

        builder.HasKey(x =>
            x.AdvertisementImageID);

        builder.Property(x =>
                x.AdvertisementImageID)
            .HasColumnName("AdvertisementImageID")
            .ValueGeneratedOnAdd();

        builder.Property(x =>
                x.AdvertisementID)
            .HasColumnName("AdvertisementID")
            .IsRequired();

        builder.Property(x =>
                x.FileName)
            .HasColumnName("FileName")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x =>
                x.S3Key)
            .HasColumnName("S3Key")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x =>
                x.FileURL)
            .HasColumnName("FileURL")
            .HasMaxLength(1500)
            .IsRequired(false);

        builder.Property(x =>
                x.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .IsRequired();

        builder.Property(x =>
                x.IsPrimary)
            .HasColumnName("IsPrimary")
            .IsRequired();

        builder.Property(x =>
                x.FileSize)
            .HasColumnName("FileSize")
            .IsRequired(false);

        builder.Property(x =>
                x.Width)
            .HasColumnName("Width")
            .IsRequired(false);

        builder.Property(x =>
                x.Height)
            .HasColumnName("Height")
            .IsRequired(false);

        builder.Property(x =>
                x.CreatedDate)
            .HasColumnName("CreatedDate")
            .IsRequired();

        builder.HasOne(x =>
                x.Advertisement)
            .WithMany(x =>
                x.Images)
            .HasForeignKey(x =>
                x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}