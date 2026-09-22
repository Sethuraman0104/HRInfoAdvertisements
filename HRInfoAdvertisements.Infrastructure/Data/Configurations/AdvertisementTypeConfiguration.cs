using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementTypeConfiguration
    : IEntityTypeConfiguration<AdvertisementType>
{
    public void Configure(EntityTypeBuilder<AdvertisementType> builder)
    {
        builder.ToTable("AdvertisementTypes");

        builder.HasKey(x => x.AdvertisementTypeID);

        builder.Property(x => x.TypeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TypeNameAr)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.TypeName)
            .IsUnique();
    }
}