using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageEntitlementContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageEntitlementContributions",
                columns: table => new
                {
                    PackageEntitlementContributionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPackageEntitlementID = table.Column<long>(type: "bigint", nullable: false),
                    PaymentTransactionID = table.Column<long>(type: "bigint", nullable: true),
                    IsLegacyBaseline = table.Column<bool>(type: "bit", nullable: false),
                    EnquiryPackageID = table.Column<long>(type: "bigint", nullable: false),
                    PackageCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MonthlyAdvertisementPostLimit = table.Column<int>(type: "int", nullable: false),
                    MonthlySenderEnquiryLimit = table.Column<int>(type: "int", nullable: false),
                    MonthlyAdvertisementEnquiryLimit = table.Column<int>(type: "int", nullable: false),
                    MonthlyReplyLimit = table.Column<int>(type: "int", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    ContributionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContributionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEntitlementContributions", x => x.PackageEntitlementContributionID);
                    table.CheckConstraint("CK_PackageEntitlementContributions_Limits", "[MonthlyAdvertisementPostLimit] >= 0 AND [MonthlySenderEnquiryLimit] >= 0 AND [MonthlyAdvertisementEnquiryLimit] >= 0 AND [MonthlyReplyLimit] >= 0");
                    table.CheckConstraint("CK_PackageEntitlementContributions_Validity", "[ValidityDays] > 0 AND [ContributionEndDate] > [ContributionStartDate]");
                    table.ForeignKey(
                        name: "FK_PackageEntitlementContributions_PaymentTransactions_PaymentTransactionID",
                        column: x => x.PaymentTransactionID,
                        principalTable: "PaymentTransactions",
                        principalColumn: "PaymentTransactionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageEntitlementContributions_UserPackageEntitlements_UserPackageEntitlementID",
                        column: x => x.UserPackageEntitlementID,
                        principalTable: "UserPackageEntitlements",
                        principalColumn: "UserPackageEntitlementID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageEntitlementContributions_PaymentTransactionID",
                table: "PackageEntitlementContributions",
                column: "PaymentTransactionID",
                unique: true,
                filter: "[PaymentTransactionID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEntitlementContributions_UserPackageEntitlementID",
                table: "PackageEntitlementContributions",
                column: "UserPackageEntitlementID",
                unique: true,
                filter: "[IsLegacyBaseline] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEntitlementContributions_UserPackageEntitlementID_ContributionStartDate_ContributionEndDate",
                table: "PackageEntitlementContributions",
                columns: new[] { "UserPackageEntitlementID", "ContributionStartDate", "ContributionEndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageEntitlementContributions");
        }
    }
}
