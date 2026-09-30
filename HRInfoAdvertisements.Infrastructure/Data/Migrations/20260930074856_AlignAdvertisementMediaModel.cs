using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class AlignAdvertisementMediaModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Advertisement media database schema is already aligned
            // with the current EF Core model.
            //
            // AdvertisementImages:
            //   FileName
            //   FileURL
            //   StorageKey
            //   ContentType
            //   FileSize
            //   IsPrimary
            //   DisplayOrder
            //   CreatedDate
            //
            // AdvertisementVideos:
            //   FileName
            //   FileURL
            //   StorageKey
            //   ContentType
            //   FileSize
            //   DisplayOrder
            //   CreatedDate
            //
            // AdvertisementDocuments:
            //   DocumentName
            //   FileURL
            //   StorageKey
            //   ContentType
            //   FileSize
            //   CreatedDate
            //
            // No database schema changes are required.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
        }
    }
}