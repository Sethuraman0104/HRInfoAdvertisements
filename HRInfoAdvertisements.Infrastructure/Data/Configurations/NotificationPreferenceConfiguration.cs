using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class NotificationPreferenceConfiguration
    : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(
        EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(x =>
            x.NotificationPreferenceID);

        builder.Property(x =>
            x.NotificationPreferenceID)
            .ValueGeneratedOnAdd();

        builder.Property(x =>
            x.UserID)
            .IsRequired();

        builder.Property(x =>
            x.EmailNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x =>
            x.AdvertisementUpdatesEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x =>
            x.FavoriteAdvertisementUpdatesEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x =>
            x.CreatedDate)
            .IsRequired();

        builder.Property(x =>
            x.ModifiedDate)
            .IsRequired(false);

        // One preference record per user.
        builder.HasIndex(x =>
            x.UserID)
            .IsUnique();

        builder.HasOne(x =>
            x.User)
            .WithOne(x =>
                x.NotificationPreference)
            .HasForeignKey<NotificationPreference>(x =>
                x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}