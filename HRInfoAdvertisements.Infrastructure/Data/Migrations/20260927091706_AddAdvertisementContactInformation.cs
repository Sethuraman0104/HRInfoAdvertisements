using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementContactInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Advertisements",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowEmailToPublic",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowWhatsAppToPublic",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "Advertisements",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ShowEmailToPublic",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ShowWhatsAppToPublic",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "Advertisements");
        }
    }
}