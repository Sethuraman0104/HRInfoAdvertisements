using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class AlignRejectionReasonColumns : Migration
    {
        
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.RejectionReasons', N'U') IS NOT NULL
BEGIN
    DELETE FROM dbo.RejectionReasons;

    IF EXISTS (
        SELECT 1 FROM sys.key_constraints
        WHERE name = N'UX_RejectionReasons_Code'
          AND parent_object_id = OBJECT_ID(N'dbo.RejectionReasons')
    )
    BEGIN
        ALTER TABLE dbo.RejectionReasons
        DROP CONSTRAINT UX_RejectionReasons_Code;
    END;

    IF COL_LENGTH(N'dbo.RejectionReasons', N'ReasonCode') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.RejectionReasons
        ALTER COLUMN ReasonCode nvarchar(50) NOT NULL;
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.key_constraints
        WHERE name = N'UX_RejectionReasons_Code'
          AND parent_object_id = OBJECT_ID(N'dbo.RejectionReasons')
    )
    BEGIN
        ALTER TABLE dbo.RejectionReasons
        ADD CONSTRAINT UX_RejectionReasons_Code
        UNIQUE NONCLUSTERED (ReasonCode);
    END;

    IF COL_LENGTH(N'dbo.RejectionReasons', N'ReasonText') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.RejectionReasons
        ALTER COLUMN ReasonText nvarchar(max) NOT NULL;
    END;

    IF COL_LENGTH(N'dbo.RejectionReasons', N'ReasonName') IS NOT NULL
        ALTER TABLE dbo.RejectionReasons DROP COLUMN ReasonName;

    IF COL_LENGTH(N'dbo.RejectionReasons', N'ReasonNameAr') IS NOT NULL
        ALTER TABLE dbo.RejectionReasons DROP COLUMN ReasonNameAr;

    IF COL_LENGTH(N'dbo.RejectionReasons', N'Description') IS NOT NULL
        ALTER TABLE dbo.RejectionReasons DROP COLUMN Description;
END;
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