using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("States");

        builder.HasKey(x => x.StateID);

        builder.Property(x => x.StateName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.StateNameAr)
            .HasMaxLength(150);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.States)
            .HasForeignKey(x => x.CountryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.CountryID,
            x.StateName
        }).IsUnique();
    }
}