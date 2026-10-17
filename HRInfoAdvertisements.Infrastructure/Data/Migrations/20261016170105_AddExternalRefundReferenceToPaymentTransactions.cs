using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalRefundReferenceToPaymentTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalRefundReference",
                table: "PaymentTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ExternalRefundReference",
                table: "PaymentTransactions",
                column: "ExternalRefundReference",
                unique: true,
                filter: "[ExternalRefundReference] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ExternalRefundReference",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ExternalRefundReference",
                table: "PaymentTransactions");
        }
    }
}
