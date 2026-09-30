using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignApprovalRequestLegacyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ------------------------------------------------------------
            // Remove legacy foreign keys
            // ------------------------------------------------------------

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_SubmittedBy",
                table: "ApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_AssignedTo",
                table: "ApprovalRequests");


            // ------------------------------------------------------------
            // SubmittedByUserID
            //
            // The existing Home database contains this column as nullable.
            // All 7 existing rows have a valid SubmittedByUserID value.
            //
            // Use raw SQL here instead of AlterColumn() because EF's
            // generated AlterColumn operation attempts to drop an index
            // that does not physically exist in the Home database.
            // ------------------------------------------------------------

            migrationBuilder.Sql(
                """
                ALTER TABLE dbo.ApprovalRequests
                ALTER COLUMN SubmittedByUserID bigint NOT NULL;
                """);


            // ------------------------------------------------------------
            // Remove obsolete legacy columns
            // ------------------------------------------------------------

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "ApprovalRequests");


            // ------------------------------------------------------------
            // Create the canonical SubmittedByUserID index.
            //
            // This index exists in the EF model snapshot but was missing
            // from the Home database.
            // ------------------------------------------------------------

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_SubmittedByUserID",
                table: "ApprovalRequests",
                column: "SubmittedByUserID");


            // ------------------------------------------------------------
            // Recreate canonical foreign keys
            // ------------------------------------------------------------

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_SubmittedByUserID",
                table: "ApprovalRequests",
                column: "SubmittedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_AssignedToUserID",
                table: "ApprovalRequests",
                column: "AssignedToUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ------------------------------------------------------------
            // Remove canonical foreign keys
            // ------------------------------------------------------------

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_SubmittedByUserID",
                table: "ApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_AssignedToUserID",
                table: "ApprovalRequests");


            // ------------------------------------------------------------
            // Remove canonical index
            // ------------------------------------------------------------

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_SubmittedByUserID",
                table: "ApprovalRequests");


            // ------------------------------------------------------------
            // Restore legacy columns
            // ------------------------------------------------------------

            migrationBuilder.AddColumn<long>(
                name: "SubmittedBy",
                table: "ApprovalRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long?>(
                name: "AssignedTo",
                table: "ApprovalRequests",
                type: "bigint",
                nullable: true);


            // ------------------------------------------------------------
            // Restore legacy data from canonical columns
            // ------------------------------------------------------------

            migrationBuilder.Sql(
                """
                UPDATE dbo.ApprovalRequests
                SET
                    SubmittedBy = SubmittedByUserID,
                    AssignedTo = AssignedToUserID;
                """);


            // ------------------------------------------------------------
            // Restore SubmittedByUserID nullable state.
            //
            // Use raw SQL to avoid EF trying to manipulate an index that
            // may not exist in the legacy database.
            // ------------------------------------------------------------

            migrationBuilder.Sql(
                """
                ALTER TABLE dbo.ApprovalRequests
                ALTER COLUMN SubmittedByUserID bigint NULL;
                """);


            // ------------------------------------------------------------
            // Restore legacy foreign keys
            // ------------------------------------------------------------

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_SubmittedBy",
                table: "ApprovalRequests",
                column: "SubmittedBy",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_AssignedTo",
                table: "ApprovalRequests",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}