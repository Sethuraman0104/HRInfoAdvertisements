using HRInfoAdvertisements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRInfoAdvertisements.Infrastructure.Data.Configurations;

public class ApprovalRequestConfiguration
    : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequests");

        builder.HasKey(x => x.ApprovalRequestID);

        // ============================================================
        // Database column mappings
        // These names match the canonical ApprovalRequests table.
        // ============================================================

        builder.Property(x => x.SubmittedByUserID)
            .HasColumnName("SubmittedByUserID")
            .IsRequired();

        builder.Property(x => x.AssignedToUserID)
            .HasColumnName("AssignedToUserID");

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Comments)
            .HasMaxLength(2000);

        builder.Property(x => x.SubmittedDate)
            .IsRequired();

        builder.Property(x => x.CompletedDate)
            .HasColumnName("CompletedDate");

        // ============================================================
        // Indexes
        // ============================================================

        builder.HasIndex(x => new
        {
            x.Status,
            x.SubmittedDate
        });

        // ============================================================
        // Advertisement relationship
        // ============================================================

        builder.HasOne(x => x.Advertisement)
            .WithMany(x => x.ApprovalRequests)
            .HasForeignKey(x => x.AdvertisementID)
            .OnDelete(DeleteBehavior.Cascade);

        // ============================================================
        // Submitted By
        // ============================================================

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany()
            .HasForeignKey(x => x.SubmittedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================================================
        // Assigned To
        // ============================================================

        builder.HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}