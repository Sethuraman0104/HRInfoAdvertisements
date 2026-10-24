using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementFeatureOptionsAndValidationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FeatureValue",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValueAr",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
                        migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "AdvertisementFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "AdvertisementFeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);
migrationBuilder.AddColumn<string>(
                name: "HelpText",
                table: "AdvertisementFeatures",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelpTextAr",
                table: "AdvertisementFeatures",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFilterable",
                table: "AdvertisementFeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSearchable",
                table: "AdvertisementFeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxLength",
                table: "AdvertisementFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "AdvertisementFeatures",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinLength",
                table: "AdvertisementFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValue",
                table: "AdvertisementFeatures",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationPattern",
                table: "AdvertisementFeatures",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryID",
                table: "AdvertisementCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdvertisementFeatureOptions",
                columns: table => new
                {
                    AdvertisementFeatureOptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementFeatureID = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OptionValueAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementFeatureOptions", x => x.AdvertisementFeatureOptionID);
                    table.ForeignKey(
                        name: "FK_AdvertisementFeatureOptions_AdvertisementFeatures_AdvertisementFeatureID",
                        column: x => x.AdvertisementFeatureID,
                        principalTable: "AdvertisementFeatures",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementCategories_ParentCategoryID",
                table: "AdvertisementCategories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementFeatureOptions_AdvertisementFeatureID_OptionValue",
                table: "AdvertisementFeatureOptions",
                columns: new[] { "AdvertisementFeatureID", "OptionValue" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementCategories_AdvertisementCategories_ParentCategoryID",
                table: "AdvertisementCategories",
                column: "ParentCategoryID",
                principalTable: "AdvertisementCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
                    migrationBuilder.CreateIndex(
                name: "IX_AdvertisementFeatures_CategoryID",
                table: "AdvertisementFeatures",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures",
                column: "CategoryID",
                principalTable: "AdvertisementCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FeatureValue",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "FeatureValueAr",
                table: "AdvertisementFeatureValues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);
                        migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementFeatures_AdvertisementCategories_CategoryID",
                table: "AdvertisementFeatures");

            migrationBuilder.DropIndex(
                name: "IX_AdvertisementFeatures_CategoryID",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "AdvertisementFeatures");
migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementCategories_AdvertisementCategories_ParentCategoryID",
                table: "AdvertisementCategories");

            migrationBuilder.DropTable(
                name: "AdvertisementFeatureOptions");

            migrationBuilder.DropIndex(
                name: "IX_AdvertisementCategories_ParentCategoryID",
                table: "AdvertisementCategories");

            migrationBuilder.DropColumn(
                name: "HelpText",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "HelpTextAr",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "IsFilterable",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "IsSearchable",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "MaxLength",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "MinLength",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "ValidationPattern",
                table: "AdvertisementFeatures");

            migrationBuilder.DropColumn(
                name: "ParentCategoryID",
                table: "AdvertisementCategories");
        }
    }
}
