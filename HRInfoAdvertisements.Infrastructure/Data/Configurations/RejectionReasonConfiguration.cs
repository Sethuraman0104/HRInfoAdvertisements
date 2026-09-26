using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class RejectionReasonConfiguration
    : IEntityTypeConfiguration<RejectionReason>
{
    public void Configure(EntityTypeBuilder<RejectionReason> builder)
    {
        builder.ToTable("RejectionReasons");

        builder.HasKey(x => x.RejectionReasonID);

        builder.Property(x => x.RejectionReasonID)
            .HasColumnName("RejectionReasonID");

        builder.Property(x => x.ReasonCode)
            .HasColumnName("ReasonCode")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReasonText)
            .HasColumnName("ReasonName")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.ReasonTextAr)
            .HasColumnName("ReasonNameAr")
            .HasMaxLength(250);

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .IsRequired();
    }
}