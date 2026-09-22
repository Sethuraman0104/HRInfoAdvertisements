using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class SavedSearchConfiguration
    : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> builder)
    {
        builder.ToTable("SavedSearches");

        builder.HasKey(x => x.SavedSearchID);

        builder.Property(x => x.SearchName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SearchCriteriaJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.UserID);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}