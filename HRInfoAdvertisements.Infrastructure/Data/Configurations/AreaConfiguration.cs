using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");

        builder.HasKey(x => x.AreaID);

        builder.Property(x => x.AreaName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AreaNameAr)
            .HasMaxLength(150);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Areas)
            .HasForeignKey(x => x.CountryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.State)
            .WithMany(x => x.Areas)
            .HasForeignKey(x => x.StateID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany(x => x.Areas)
            .HasForeignKey(x => x.CityID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.CountryID,
            x.StateID,
            x.CityID,
            x.AreaName
        }).IsUnique();
    }
}