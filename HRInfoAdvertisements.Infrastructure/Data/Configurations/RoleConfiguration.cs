using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.RoleID);

        builder.Property(x => x.RoleName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RoleDescription)
            .HasMaxLength(500);

        builder.HasIndex(x => x.RoleName)
            .IsUnique();
    }
}