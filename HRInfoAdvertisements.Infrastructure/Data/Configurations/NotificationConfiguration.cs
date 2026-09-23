using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.NotificationID);

        builder.Property(x => x.NotificationID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserID)
            .IsRequired();

        builder.Property(x => x.NotificationTemplateID)
            .IsRequired(false);

        builder.Property(x => x.NotificationType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.ReferenceID)
            .IsRequired(false);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReadDate)
            .IsRequired(false);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserID,
            x.IsRead,
            x.CreatedDate
        });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NotificationTemplate)
            .WithMany()
            .HasForeignKey(x => x.NotificationTemplateID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}