using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.CountryID);

        builder.Property(x => x.CountryCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CountryName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CountryNameAr)
            .HasMaxLength(150);

        builder.HasIndex(x => x.CountryCode)
            .IsUnique();

        builder.HasIndex(x => x.CountryName)
            .IsUnique();
    }
}