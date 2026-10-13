using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnquiryPackageValidityDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValidityDays",
                table: "EnquiryPackages",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnquiryPackages_ValidityDays",
                table: "EnquiryPackages",
                sql: "[ValidityDays] IS NULL OR [ValidityDays] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EnquiryPackages_ValidityDays",
                table: "EnquiryPackages");

            migrationBuilder.DropColumn(
                name: "ValidityDays",
                table: "EnquiryPackages");
        }
    }
}
