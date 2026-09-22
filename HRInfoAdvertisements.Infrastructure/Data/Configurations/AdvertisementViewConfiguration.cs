using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementViewConfiguration
    : IEntityTypeConfiguration<AdvertisementView>
{
    public void Configure(EntityTypeBuilder<AdvertisementView> builder)
    {
        builder.ToTable("AdvertisementViews");

        builder.HasKey(x => x.AdvertisementViewID);

        builder.Property(x => x.IPAddress)
            .HasMaxLength(100);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(1000);

        builder.HasIndex(x => new
        {
            x.AdvertisementID,
            x.ViewedDate
        });

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Views)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}