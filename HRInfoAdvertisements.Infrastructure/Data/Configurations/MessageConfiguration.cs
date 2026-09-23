using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.MessageID);

        builder.Property(x => x.MessageID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SenderUserID)
            .IsRequired();

        builder.Property(x => x.ReceiverUserID)
            .IsRequired();

        builder.Property(x => x.AdvertisementID)
            .IsRequired(false);

        builder.Property(x => x.EnquiryID)
            .IsRequired(false);

        builder.Property(x => x.MessageText)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReadDate)
            .IsRequired(false);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        // Sender
        builder.HasOne(x => x.SenderUser)
            .WithMany()
            .HasForeignKey(x => x.SenderUserID)
            .OnDelete(DeleteBehavior.NoAction);

        // Receiver
        builder.HasOne(x => x.ReceiverUser)
            .WithMany()
            .HasForeignKey(x => x.ReceiverUserID)
            .OnDelete(DeleteBehavior.NoAction);

        // Advertisement
        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.NoAction);

        // Enquiry
        builder.HasOne(x => x.Enquiry)
            .WithMany()
            .HasForeignKey(x => x.EnquiryID)
            .OnDelete(DeleteBehavior.NoAction);

        // Receiver unread-message lookup
        builder.HasIndex(x => new
        {
            x.ReceiverUserID,
            x.IsRead,
            x.CreatedDate
        });
    }
}