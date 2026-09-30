using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class AlignRejectionReasonColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * The Home database contains the legacy RejectionReasons schema.
             *
             * Existing legacy columns:
             *   ReasonName
             *   ReasonNameAr
             *   Description
             *
             * The current application model uses:
             *   ReasonCode
             *   ReasonText
             *   ReasonTextAr
             *   IsActive
             *   DisplayOrder
             *
             * There are no foreign-key references to RejectionReasons.
             * The existing six legacy rows can therefore be replaced by
             * the canonical seed data in SeedRejectionReasons.
             */

            /*
             * Remove the old six rows.
             *
             * SeedRejectionReasons will subsequently insert the canonical
             * rows using IDs 1 through 10.
             */
            migrationBuilder.Sql(@"
                DELETE FROM [dbo].[RejectionReasons];
            ");

            /*
             * UX_RejectionReasons_Code is a UNIQUE CONSTRAINT, not a
             * standalone index. It must therefore be dropped as a
             * constraint before altering ReasonCode.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                DROP CONSTRAINT [UX_RejectionReasons_Code];
            ");

            /*
             * ReasonCode remains a bounded string because it is a unique
             * business code and participates in a UNIQUE constraint.
             *
             * Existing:
             *     varchar(50)
             *
             * Canonical application/database representation:
             *     nvarchar(50)
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonCode] nvarchar(50) NOT NULL;
            ");

            /*
             * Restore the unique constraint.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ADD CONSTRAINT [UX_RejectionReasons_Code]
                UNIQUE NONCLUSTERED ([ReasonCode]);
            ");

            /*
             * ReasonText is part of the current EF model and is required.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonText] nvarchar(max) NOT NULL;
            ");

            /*
             * Remove legacy columns no longer used by the current EF model.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [ReasonName];

                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [ReasonNameAr];

                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [Description];
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
             * Restore the legacy columns.
             */
            migrationBuilder.AddColumn<string>(
                name: "ReasonName",
                table: "RejectionReasons",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonNameAr",
                table: "RejectionReasons",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RejectionReasons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            /*
             * Remove the unique constraint before changing ReasonCode
             * back to the legacy varchar(50) representation.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                DROP CONSTRAINT [UX_RejectionReasons_Code];
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonCode] varchar(50) NOT NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ADD CONSTRAINT [UX_RejectionReasons_Code]
                UNIQUE NONCLUSTERED ([ReasonCode]);
            ");

            /*
             * Restore the previous nullable state of ReasonText.
             */
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonText] nvarchar(max) NULL;
            ");
        }
    }
}