using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagePurchaseRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackagePurchaseRequests",
                columns: table => new
                {
                    PackagePurchaseRequestID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    EnquiryPackageID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagePurchaseRequests", x => x.PackagePurchaseRequestID);
                    table.CheckConstraint("CK_PackagePurchaseRequests_Amount", "[Amount] >= 0");
                    table.CheckConstraint("CK_PackagePurchaseRequests_Status", "[Status] IN ('PENDING', 'APPROVED', 'COMPLETED', 'REJECTED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "FK_PackagePurchaseRequests_EnquiryPackages_EnquiryPackageID",
                        column: x => x.EnquiryPackageID,
                        principalTable: "EnquiryPackages",
                        principalColumn: "EnquiryPackageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseRequests_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseRequests_EnquiryPackageID",
                table: "PackagePurchaseRequests",
                column: "EnquiryPackageID");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseRequests_UserID",
                table: "PackagePurchaseRequests",
                column: "UserID",
                unique: true,
                filter: "[Status] = 'PENDING'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagePurchaseRequests");
        }
    }
}
