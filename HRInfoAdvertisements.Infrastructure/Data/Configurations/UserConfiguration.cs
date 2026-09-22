using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.UserID);

        builder.Property(x => x.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.MobileNo)
            .HasMaxLength(30);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.AccountStatus)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.HasIndex(x => x.MobileNo)
            .IsUnique()
            .HasFilter("[MobileNo] IS NOT NULL");

        builder.HasOne(x => x.UserProfile)
            .WithOne(x => x.User)
            .HasForeignKey<UserProfile>(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}