using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class AlignRejectionReasonColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The RejectionReasons database schema is already aligned
            // with the current EF Core model.
            //
            // The database uses:
            //   ReasonText
            //   ReasonTextAr
            //
            // No database schema changes are required.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
        }
    }
}