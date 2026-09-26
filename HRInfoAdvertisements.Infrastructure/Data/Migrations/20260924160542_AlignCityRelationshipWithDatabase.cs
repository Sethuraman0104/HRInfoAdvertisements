using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignCityRelationshipWithDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            //
            // The existing database already contains the desired
            // Countries -> States -> Cities relationship.
            //
            // Cities does not contain CountryID.
            // Cities.StateID -> States.StateID
            // States.CountryID -> Countries.CountryID
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