using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class UserSessionConfiguration
    : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(x => x.UserSessionID);

        builder.Property(x => x.SessionTokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.DeviceName)
            .HasMaxLength(200);

        builder.Property(x => x.DeviceType)
            .HasMaxLength(50);

        builder.Property(x => x.IPAddress)
            .HasMaxLength(100);

        builder.HasIndex(x => x.SessionTokenHash)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}