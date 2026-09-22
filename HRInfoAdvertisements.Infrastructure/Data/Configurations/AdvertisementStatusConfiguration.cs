using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementStatusConfiguration
    : IEntityTypeConfiguration<AdvertisementStatus>
{
    public void Configure(EntityTypeBuilder<AdvertisementStatus> builder)
    {
        builder.ToTable("AdvertisementStatuses");

        builder.HasKey(x => x.StatusID);

        builder.Property(x => x.StatusCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StatusName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.StatusNameAr)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.StatusCode)
            .IsUnique();
    }
}