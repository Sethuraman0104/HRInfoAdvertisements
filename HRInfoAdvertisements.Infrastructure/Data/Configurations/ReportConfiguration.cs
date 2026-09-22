using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(x => x.ReportID);

        builder.Property(x => x.Comments)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Status,
            x.CreatedDate
        });

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Reports)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReportedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReportedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ReportReason)
            .WithMany(x => x.Reports)
            .HasForeignKey(x => x.ReportReasonID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}