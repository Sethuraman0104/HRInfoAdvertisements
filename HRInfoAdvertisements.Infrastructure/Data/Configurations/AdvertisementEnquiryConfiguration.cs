using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class AdvertisementEnquiryConfiguration
    : IEntityTypeConfiguration<AdvertisementEnquiry>
{
    public void Configure(EntityTypeBuilder<AdvertisementEnquiry> builder)
    {
        builder.ToTable("AdvertisementEnquiries");

        builder.HasKey(x => x.AdvertisementEnquiryID);

        builder.Property(x => x.Message)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(x => x.ContactMobile)
            .HasMaxLength(30);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AdvertisementID,
            x.CreatedDate
        });

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Enquiries)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SenderUser)
            .WithMany()
            .HasForeignKey(x => x.SenderUserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}