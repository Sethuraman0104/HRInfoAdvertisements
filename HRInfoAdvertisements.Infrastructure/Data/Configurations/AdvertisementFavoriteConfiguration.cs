using HRInfoAdvertisements.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementFavoriteConfiguration
    : IEntityTypeConfiguration<AdvertisementFavorite>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementFavorite> builder)
    {
        builder.ToTable("AdvertisementFavorites");

        builder.HasKey(x =>
            x.AdvertisementFavoriteID);

        builder.Property(x =>
            x.CreatedDate)
            .IsRequired();

        // One user cannot favorite
        // the same advertisement twice.
        builder.HasIndex(x => new
        {
            x.UserID,
            x.AdvertisementID
        })
        .IsUnique();

        builder.HasOne(x =>
            x.User)
            .WithMany()
            .HasForeignKey(x =>
                x.UserID)
            .OnDelete(
                DeleteBehavior.Cascade);

        builder.HasOne(x =>
            x.Advertisement)
            .WithMany()
            .HasForeignKey(x =>
                x.AdvertisementID)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}