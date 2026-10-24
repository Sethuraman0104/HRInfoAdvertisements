using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class RemoveRedundantAdvertisementFeatureValueIDColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvertisementFeatureID",
                table: "AdvertisementFeatureValues");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvertisementFeatureID",
                table: "AdvertisementFeatureValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE dbo.AdvertisementFeatureValues " +
                "SET AdvertisementFeatureID = FeatureID;");
        }
    }
}