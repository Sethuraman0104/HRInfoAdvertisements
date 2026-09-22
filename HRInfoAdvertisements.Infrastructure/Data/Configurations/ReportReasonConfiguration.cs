using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class ReportReasonConfiguration
    : IEntityTypeConfiguration<ReportReason>
{
    public void Configure(EntityTypeBuilder<ReportReason> builder)
    {
        builder.ToTable("ReportReasons");

        builder.HasKey(x => x.ReportReasonID);

        builder.Property(x => x.ReasonCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReasonText)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.ReasonTextAr)
            .HasMaxLength(300);

        builder.HasIndex(x => x.ReasonCode)
            .IsUnique();
    }
}