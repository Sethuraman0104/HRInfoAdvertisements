using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class OTPRequestConfiguration
    : IEntityTypeConfiguration<OTPRequest>
{
    public void Configure(EntityTypeBuilder<OTPRequest> builder)
    {
        builder.ToTable("OTPRequests");

        builder.HasKey(x => x.OTPRequestID);

        builder.Property(x => x.Destination)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.OTPHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Destination,
            x.Purpose,
            x.CreatedDate
        });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}