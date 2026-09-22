using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementDocumentConfiguration
    : IEntityTypeConfiguration<AdvertisementDocument>
{
    public void Configure(EntityTypeBuilder<AdvertisementDocument> builder)
    {
        builder.ToTable("AdvertisementDocuments");

        builder.HasKey(x => x.AdvertisementDocumentID);

        builder.Property(x => x.DocumentName)
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
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}