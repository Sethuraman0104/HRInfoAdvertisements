using HRInfoAdvertisements.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementRemovalRequestConfiguration
    : IEntityTypeConfiguration<AdvertisementRemovalRequest>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementRemovalRequest> builder)
    {
        builder.ToTable("AdvertisementRemovalRequests");

        builder.HasKey(x => x.RemovalRequestID);

        builder.Property(x => x.RequestType)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.RequestStatus)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.ReviewComments)
            .HasMaxLength(500);

        builder.HasIndex(x => new
    {
        x.AdvertisementID,
        x.CreatedDate
    })
    .HasDatabaseName("IX_AdvRemovalReq_Ad_Created")
    .IsUnique(false);

        builder.HasIndex(x => x.AdvertisementID)
            .HasDatabaseName("UX_AdvRemovalReq_OnePending")
            .IsUnique()
            .HasFilter("[RequestStatus] = 'PENDING'");
    }
}