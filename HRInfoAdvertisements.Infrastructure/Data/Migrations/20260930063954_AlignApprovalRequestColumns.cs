using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignApprovalRequestColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The ApprovalRequests database schema is already aligned
            // with the current EF Core model.
            //
            // Existing canonical columns:
            //   AdvertisementID
            //   SubmittedByUserID
            //   AssignedToUserID
            //   Status
            //   Comments
            //   SubmittedDate
            //   CompletedDate
            //
            // No database schema changes are required.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            //
            // The canonical ApprovalRequests schema existed before this
            // migration and must not be renamed back to the obsolete
            // column names.
        }
    }
}