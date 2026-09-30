using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementDocumentConfiguration
    : IEntityTypeConfiguration<AdvertisementDocument>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementDocument> builder)
    {
        builder.ToTable("AdvertisementDocuments");

        builder.HasKey(x =>
            x.AdvertisementDocumentID);

        builder.Property(x =>
                x.AdvertisementDocumentID)
            .HasColumnName("AdvertisementDocumentID")
            .ValueGeneratedOnAdd();

        builder.Property(x =>
                x.AdvertisementID)
            .HasColumnName("AdvertisementID")
            .IsRequired();

        builder.Property(x =>
                x.DocumentName)
            .HasColumnName("DocumentName")
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
                x.CreatedDate)
            .HasColumnName("CreatedDate")
            .IsRequired();

        builder.HasOne(x =>
                x.Advertisement)
            .WithMany(x =>
                x.Documents)
            .HasForeignKey(x =>
                x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}