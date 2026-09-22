using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementConfiguration
    : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements");

        builder.HasKey(x => x.AdvertisementID);

        builder.Property(x => x.AdvertisementNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.AdvertisementNumber)
            .IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.TitleAr)
            .HasMaxLength(250);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("BHD");

        builder.Property(x => x.Price)
            .HasPrecision(18, 3);

        builder.Property(x => x.LandArea)
            .HasPrecision(18, 2);

        builder.Property(x => x.BuiltUpArea)
            .HasPrecision(18, 2);

        builder.Property(x => x.Latitude)
            .HasPrecision(10, 7);

        builder.Property(x => x.Longitude)
            .HasPrecision(10, 7);

        builder.HasIndex(x => x.UserID);
        builder.HasIndex(x => x.CategoryID);
        builder.HasIndex(x => x.AdvertisementTypeID);
        builder.HasIndex(x => x.StatusID);
        builder.HasIndex(x => x.CountryID);
        builder.HasIndex(x => x.StateID);
        builder.HasIndex(x => x.CityID);
        builder.HasIndex(x => x.AreaID);
        builder.HasIndex(x => x.PublishedDate);
        builder.HasIndex(x => x.ExpiryDate);
        builder.HasIndex(x => x.Price);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AdvertisementType)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.AdvertisementTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.StatusID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.CountryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.State)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.StateID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.CityID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.AreaID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}