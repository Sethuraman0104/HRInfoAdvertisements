using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementEnquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RespondedDate",
                table: "AdvertisementEnquiries",
                newName: "LastRepliedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "AdvertisementEnquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RecipientUserID",
                table: "AdvertisementEnquiries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "AdvertisementEnquiries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AdvertisementEnquiryMessages",
                columns: table => new
                {
                    AdvertisementEnquiryMessageID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementEnquiryID = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserID = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementEnquiryMessages", x => x.AdvertisementEnquiryMessageID);
                    table.ForeignKey(
                        name: "FK_AdvertisementEnquiryMessages_AdvertisementEnquiries_AdvertisementEnquiryID",
                        column: x => x.AdvertisementEnquiryID,
                        principalTable: "AdvertisementEnquiries",
                        principalColumn: "AdvertisementEnquiryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvertisementEnquiryMessages_Users_SenderUserID",
                        column: x => x.SenderUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementEnquiries_RecipientUserID",
                table: "AdvertisementEnquiries",
                column: "RecipientUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementEnquiryMessages_AdvertisementEnquiryID_CreatedDate",
                table: "AdvertisementEnquiryMessages",
                columns: new[] { "AdvertisementEnquiryID", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementEnquiryMessages_SenderUserID_IsRead",
                table: "AdvertisementEnquiryMessages",
                columns: new[] { "SenderUserID", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementEnquiries_Users_RecipientUserID",
                table: "AdvertisementEnquiries",
                column: "RecipientUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementEnquiries_Users_RecipientUserID",
                table: "AdvertisementEnquiries");

            migrationBuilder.DropTable(
                name: "AdvertisementEnquiryMessages");

            migrationBuilder.DropIndex(
                name: "IX_AdvertisementEnquiries_RecipientUserID",
                table: "AdvertisementEnquiries");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "AdvertisementEnquiries");

            migrationBuilder.DropColumn(
                name: "RecipientUserID",
                table: "AdvertisementEnquiries");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "AdvertisementEnquiries");

            migrationBuilder.RenameColumn(
                name: "LastRepliedDate",
                table: "AdvertisementEnquiries",
                newName: "RespondedDate");
        }
    }
}
