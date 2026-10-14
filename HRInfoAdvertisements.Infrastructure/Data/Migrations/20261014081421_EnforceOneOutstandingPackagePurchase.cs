using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOneOutstandingPackagePurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackagePurchaseRequests_UserID",
                table: "PackagePurchaseRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseRequests_UserID",
                table: "PackagePurchaseRequests",
                column: "UserID",
                unique: true,
                filter: "[Status] IN ('PENDING', 'APPROVED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackagePurchaseRequests_UserID",
                table: "PackagePurchaseRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseRequests_UserID",
                table: "PackagePurchaseRequests",
                column: "UserID",
                unique: true,
                filter: "[Status] = 'PENDING'");
        }
    }
}
