using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class LoginHistoryConfiguration
    : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistories");

        builder.HasKey(x => x.LoginHistoryID);

        builder.Property(x => x.LoginIdentifier)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.IPAddress)
            .HasMaxLength(100);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.LoginDate);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}