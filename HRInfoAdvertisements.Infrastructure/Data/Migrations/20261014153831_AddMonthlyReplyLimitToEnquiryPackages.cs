using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyReplyLimitToEnquiryPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyReplyLimit",
                table: "EnquiryPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnquiryPackages_ReplyLimit",
                table: "EnquiryPackages",
                sql: "[MonthlyReplyLimit] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EnquiryPackages_ReplyLimit",
                table: "EnquiryPackages");

            migrationBuilder.DropColumn(
                name: "MonthlyReplyLimit",
                table: "EnquiryPackages");
        }
    }
}
