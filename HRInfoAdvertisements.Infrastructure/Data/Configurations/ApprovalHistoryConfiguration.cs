using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class ApprovalHistoryConfiguration
    : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistories");

        builder.HasKey(x => x.ApprovalHistoryID);

        builder.Property(x => x.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Comments)
            .HasMaxLength(2000);

        builder.HasOne(x => x.ApprovalRequest)
            .WithMany(x => x.ApprovalHistory)
            .HasForeignKey(x => x.ApprovalRequestID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActionedByUser)
            .WithMany()
            .HasForeignKey(x => x.ActionedByUserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}