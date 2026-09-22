using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.AuditLogID);

        builder.Property(x => x.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.EntityID)
            .HasMaxLength(100);

        builder.Property(x => x.IPAddress)
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.EntityName,
            x.EntityID
        });

        builder.HasIndex(x => x.CreatedDate);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}