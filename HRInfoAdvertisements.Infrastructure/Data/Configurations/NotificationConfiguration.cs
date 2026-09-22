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

        builder.Property(x => x.NotificationType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(2000);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100);

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