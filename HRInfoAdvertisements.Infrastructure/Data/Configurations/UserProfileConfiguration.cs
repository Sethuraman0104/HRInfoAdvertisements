using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(x => x.UserProfileID);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.ProfilePhotoURL)
            .HasMaxLength(1000);

        builder.Property(x => x.Nationality)
            .HasMaxLength(100);

        builder.Property(x => x.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(x => x.CompanyName)
            .HasMaxLength(250);

        builder.HasIndex(x => x.UserID)
            .IsUnique();
    }
}