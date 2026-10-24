using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations.ReviewTemp
{
    /// <inheritdoc />
    public partial class ReviewAdvertisementFeatureModelDiff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementFeatureValues_AdvertisementFeatures_AdvertisementFeatureID",
                table: "AdvertisementFeatureValues");

            migrationBuilder.RenameColumn(
                name: "AdvertisementFeatureID",
                table: "AdvertisementFeatureValues",
                newName: "FeatureID");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementFeatureValues_AdvertisementID_AdvertisementFeatureID",
                table: "AdvertisementFeatureValues",
                newName: "IX_AdvertisementFeatureValues_AdvertisementID_FeatureID");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementFeatureValues_AdvertisementFeatureID",
                table: "AdvertisementFeatureValues",
                newName: "IX_AdvertisementFeatureValues_FeatureID");

            migrationBuilder.RenameColumn(
                name: "AdvertisementFeatureID",
                table: "AdvertisementFeatures",
                newName: "FeatureID");

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValueAr",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValue",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DataType",
                table: "AdvertisementFeatures",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryID",
                table: "AdvertisementFeatures",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures",
                column: "CategoryID",
                principalTable: "AdvertisementCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementFeatureValues_AdvertisementFeatures_FeatureID",
                table: "AdvertisementFeatureValues",
                column: "FeatureID",
                principalTable: "AdvertisementFeatures",
                principalColumn: "FeatureID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementFeatureValues_AdvertisementFeatures_FeatureID",
                table: "AdvertisementFeatureValues");

            migrationBuilder.RenameColumn(
                name: "FeatureID",
                table: "AdvertisementFeatureValues",
                newName: "AdvertisementFeatureID");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementFeatureValues_FeatureID",
                table: "AdvertisementFeatureValues",
                newName: "IX_AdvertisementFeatureValues_AdvertisementFeatureID");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementFeatureValues_AdvertisementID_FeatureID",
                table: "AdvertisementFeatureValues",
                newName: "IX_AdvertisementFeatureValues_AdvertisementID_AdvertisementFeatureID");

            migrationBuilder.RenameColumn(
                name: "FeatureID",
                table: "AdvertisementFeatures",
                newName: "AdvertisementFeatureID");

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValueAr",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValue",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DataType",
                table: "AdvertisementFeatures",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryID",
                table: "AdvertisementFeatures",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures",
                column: "CategoryID",
                principalTable: "AdvertisementCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementFeatureValues_AdvertisementFeatures_AdvertisementFeatureID",
                table: "AdvertisementFeatureValues",
                column: "AdvertisementFeatureID",
                principalTable: "AdvertisementFeatures",
                principalColumn: "AdvertisementFeatureID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
