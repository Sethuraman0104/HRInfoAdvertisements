using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class RemoveRedundantAdvertisementFeatureIDColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.AdvertisementFeatures', N'AdvertisementFeatureID') IS NOT NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.AdvertisementFeatures
        WHERE AdvertisementFeatureID IS NULL
           OR AdvertisementFeatureID <> FeatureID
    )
    BEGIN
        THROW 51000,
            'Cannot remove AdvertisementFeatureID because its values do not match FeatureID.',
            1;
    END;

    ALTER TABLE dbo.AdvertisementFeatures
        DROP COLUMN AdvertisementFeatureID;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.AdvertisementFeatures', N'AdvertisementFeatureID') IS NULL
BEGIN
    ALTER TABLE dbo.AdvertisementFeatures
        ADD AdvertisementFeatureID int NULL;

    UPDATE dbo.AdvertisementFeatures
        SET AdvertisementFeatureID = FeatureID;

    ALTER TABLE dbo.AdvertisementFeatures
        ALTER COLUMN AdvertisementFeatureID int NOT NULL;
END;
");
        }
    }
}