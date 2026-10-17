using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasedPackageLimitSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyAdvertisementEnquiryLimit",
                table: "UserPackageEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyAdvertisementPostLimit",
                table: "UserPackageEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyReplyLimit",
                table: "UserPackageEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlySenderEnquiryLimit",
                table: "UserPackageEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserPackageEntitlements_NonNegativeLimits",
                table: "UserPackageEntitlements",
                sql: "[MonthlyAdvertisementPostLimit] >= 0 AND [MonthlySenderEnquiryLimit] >= 0 AND [MonthlyAdvertisementEnquiryLimit] >= 0 AND [MonthlyReplyLimit] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserPackageEntitlements_NonNegativeLimits",
                table: "UserPackageEntitlements");

            migrationBuilder.DropColumn(
                name: "MonthlyAdvertisementEnquiryLimit",
                table: "UserPackageEntitlements");

            migrationBuilder.DropColumn(
                name: "MonthlyAdvertisementPostLimit",
                table: "UserPackageEntitlements");

            migrationBuilder.DropColumn(
                name: "MonthlyReplyLimit",
                table: "UserPackageEntitlements");

            migrationBuilder.DropColumn(
                name: "MonthlySenderEnquiryLimit",
                table: "UserPackageEntitlements");
        }
    }
}
