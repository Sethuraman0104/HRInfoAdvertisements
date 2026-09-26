using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignAreaRelationshipWithDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            //
            // The existing database already contains the desired
            // Areas schema and relationship.
            //
            // Actual database structure:
            //
            // Areas.AreaID
            // Areas.CityID
            // Areas.AreaName
            // Areas.AreaNameAr
            // Areas.IsActive
            //
            // Foreign key:
            // Areas.CityID -> Cities.CityID
            //
            // This migration updates the EF Core model snapshot only
            // and does not modify the existing database schema.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            //
            // This is a model-baseline migration.
            // No database schema changes are reversed here.
        }
    }
}