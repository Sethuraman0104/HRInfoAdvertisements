using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.CityID);

        builder.Property(x => x.CityName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CityNameAr)
            .HasMaxLength(150);

        builder.HasOne(x => x.State)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.StateID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.StateID,
            x.CityName
        })
        .IsUnique();
    }
}