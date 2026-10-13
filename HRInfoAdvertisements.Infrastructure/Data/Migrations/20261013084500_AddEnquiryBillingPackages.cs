using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnquiryBillingPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnquiryPackages",
                columns: table => new
                {
                    EnquiryPackageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MonthlySenderEnquiryLimit = table.Column<int>(type: "int", nullable: false),
                    MonthlyAdvertisementEnquiryLimit = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryPackages", x => x.EnquiryPackageID);
                    table.CheckConstraint("CK_EnquiryPackages_AdvertisementLimit", "[MonthlyAdvertisementEnquiryLimit] >= 0");
                    table.CheckConstraint("CK_EnquiryPackages_Price", "[Price] >= 0");
                    table.CheckConstraint("CK_EnquiryPackages_SenderLimit", "[MonthlySenderEnquiryLimit] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "UserPackageEntitlements",
                columns: table => new
                {
                    UserPackageEntitlementID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    EnquiryPackageID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPackageEntitlements", x => x.UserPackageEntitlementID);
                    table.CheckConstraint("CK_UserPackageEntitlements_DateRange", "[EndDate] IS NULL OR [EndDate] > [StartDate]");
                    table.CheckConstraint("CK_UserPackageEntitlements_Status", "[Status] IN ('PENDING', 'ACTIVE', 'CANCELLED', 'EXPIRED')");
                    table.ForeignKey(
                        name: "FK_UserPackageEntitlements_EnquiryPackages_EnquiryPackageID",
                        column: x => x.EnquiryPackageID,
                        principalTable: "EnquiryPackages",
                        principalColumn: "EnquiryPackageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPackageEntitlements_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PaymentTransactionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPackageEntitlementID = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderTransactionReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PaymentTransactionID);
                    table.CheckConstraint("CK_PaymentTransactions_Amount", "[Amount] >= 0");
                    table.CheckConstraint("CK_PaymentTransactions_Status", "[Status] IN ('PENDING', 'CONFIRMED', 'FAILED', 'REFUNDED')");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_UserPackageEntitlements_UserPackageEntitlementID",
                        column: x => x.UserPackageEntitlementID,
                        principalTable: "UserPackageEntitlements",
                        principalColumn: "UserPackageEntitlementID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPackages_Name",
                table: "EnquiryPackages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPackages_PackageCode",
                table: "EnquiryPackages",
                column: "PackageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentProvider_ProviderTransactionReference",
                table: "PaymentTransactions",
                columns: new[] { "PaymentProvider", "ProviderTransactionReference" },
                unique: true,
                filter: "[ProviderTransactionReference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_UserPackageEntitlementID",
                table: "PaymentTransactions",
                column: "UserPackageEntitlementID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackageEntitlements_EnquiryPackageID",
                table: "UserPackageEntitlements",
                column: "EnquiryPackageID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackageEntitlements_UserID",
                table: "UserPackageEntitlements",
                column: "UserID",
                unique: true,
                filter: "[Status] = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackageEntitlements_UserID_Status_StartDate_EndDate",
                table: "UserPackageEntitlements",
                columns: new[] { "UserID", "Status", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "UserPackageEntitlements");

            migrationBuilder.DropTable(
                name: "EnquiryPackages");
        }
    }
}
