using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class NotificationTemplateConfiguration
    : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(x => x.NotificationTemplateID);

        builder.Property(x => x.TemplateCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TemplateName)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.TemplateCode)
            .IsUnique();
    }
}