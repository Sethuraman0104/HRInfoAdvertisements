using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementCategoryConfiguration
    : IEntityTypeConfiguration<AdvertisementCategory>
{
    public void Configure(EntityTypeBuilder<AdvertisementCategory> builder)
    {
        builder.ToTable("AdvertisementCategories");

        builder.HasKey(x => x.CategoryID);

        builder.Property(x => x.CategoryName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CategoryNameAr)
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryName)
            .IsUnique();
    }
}
