using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class UserAddressConfiguration
    : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(x => x.UserAddressID);

        builder.Property(x => x.AddressLine1)
            .HasMaxLength(300);

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(300);

        builder.Property(x => x.BuildingNo)
            .HasMaxLength(50);

        builder.Property(x => x.RoadNo)
            .HasMaxLength(50);

        builder.Property(x => x.BlockNo)
            .HasMaxLength(50);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(30);

        builder.HasIndex(x => x.UserID);

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserAddresses)
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.State)
            .WithMany()
            .HasForeignKey(x => x.StateID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Area)
            .WithMany()
            .HasForeignKey(x => x.AreaID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}