using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class SystemSettingConfiguration
    : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(x => x.SystemSettingID);

        builder.Property(x => x.SettingKey)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SettingValue)
            .HasMaxLength(4000);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.SettingKey)
            .IsUnique();
    }
}