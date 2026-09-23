using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementEnquiryMessageConfiguration
    : IEntityTypeConfiguration<AdvertisementEnquiryMessage>
{
    public void Configure(
        EntityTypeBuilder<AdvertisementEnquiryMessage> builder)
    {
        builder.ToTable("AdvertisementEnquiryMessages");

        builder.HasKey(x => x.AdvertisementEnquiryMessageID);

        builder.Property(x => x.Message)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AdvertisementEnquiryID,
            x.CreatedDate
        });

        builder.HasIndex(x => new
        {
            x.SenderUserID,
            x.IsRead
        });

        builder.HasOne(x => x.AdvertisementEnquiry)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.AdvertisementEnquiryID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SenderUser)
            .WithMany()
            .HasForeignKey(x => x.SenderUserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}