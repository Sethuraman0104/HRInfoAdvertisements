
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAdvertisementPostLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyAdvertisementPostLimit",
                table: "EnquiryPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnquiryPackages_AdvertisementPostLimit",
                table: "EnquiryPackages",
                sql: "[MonthlyAdvertisementPostLimit] >= 0");

            migrationBuilder.Sql(
                """
                UPDATE dbo.EnquiryPackages
                SET MonthlyAdvertisementPostLimit = 5
                WHERE PackageCode = 'FREE';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EnquiryPackages_AdvertisementPostLimit",
                table: "EnquiryPackages");

            migrationBuilder.DropColumn(
                name: "MonthlyAdvertisementPostLimit",
                table: "EnquiryPackages");
        }
    }
}
