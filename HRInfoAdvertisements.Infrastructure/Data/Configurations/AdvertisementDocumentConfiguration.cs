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
                x.DocumentType)
            .HasColumnName("DocumentType")
            .HasMaxLength(50)
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
                x.VerificationStatus)
            .HasColumnName("VerificationStatus")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x =>
                x.VerifiedBy)
            .HasColumnName("VerifiedBy")
            .IsRequired(false);

        builder.Property(x =>
                x.VerifiedDate)
            .HasColumnName("VerifiedDate")
            .IsRequired(false);

        builder.Property(x =>
                x.RejectionReason)
            .HasColumnName("RejectionReason")
            .HasMaxLength(1000)
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