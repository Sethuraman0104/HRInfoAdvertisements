using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations;

public partial class AlignSystemSettingsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.SystemSettings', N'U') IS NULL
            BEGIN
                RETURN;
            END;

            IF OBJECT_ID(N'dbo.SystemSettings_Legacy', N'U') IS NOT NULL
            BEGIN
                DROP TABLE dbo.SystemSettings_Legacy;
            END;

            /*
             * Rename the existing primary key constraint first.
             * sp_rename on the table does not rename the constraint.
             */
            IF EXISTS
            (
                SELECT 1
                FROM sys.key_constraints
                WHERE name = N'PK_SystemSettings'
                  AND parent_object_id = OBJECT_ID(N'dbo.SystemSettings')
            )
            BEGIN
                EXEC sp_rename
                    N'dbo.PK_SystemSettings',
                    N'PK_SystemSettings_Legacy',
                    N'OBJECT';
            END;

            /*
             * Preserve the existing table/data temporarily.
             */
            EXEC sp_rename
                N'dbo.SystemSettings',
                N'SystemSettings_Legacy';

            /*
             * Create the final SystemSettings schema.
             */
            CREATE TABLE dbo.SystemSettings
            (
                SystemSettingID int IDENTITY(1,1) NOT NULL,
                SettingKey nvarchar(150) NOT NULL,
                SettingValue nvarchar(4000) NULL,
                Description nvarchar(500) NULL,
                DataType varchar(30) NULL,
                IsEncrypted bit NOT NULL,
                IsActive bit NOT NULL,
                CreatedDate datetime2 NOT NULL,
                ModifiedDate datetime2 NULL,

                CONSTRAINT PK_SystemSettings
                    PRIMARY KEY (SystemSettingID)
            );

            SET IDENTITY_INSERT dbo.SystemSettings ON;

            /*
             * Existing office database does not have DataType.
             * Preserve all existing values and initialize DataType as NULL.
             */
            INSERT INTO dbo.SystemSettings
            (
                SystemSettingID,
                SettingKey,
                SettingValue,
                Description,
                DataType,
                IsEncrypted,
                IsActive,
                CreatedDate,
                ModifiedDate
            )
            SELECT
                SystemSettingID,
                CONVERT(nvarchar(150), SettingKey),
                CONVERT(nvarchar(4000), SettingValue),
                CONVERT(nvarchar(500), Description),
                NULL,
                IsEncrypted,
                IsActive,
                CreatedDate,
                ModifiedDate
            FROM dbo.SystemSettings_Legacy
            ORDER BY SystemSettingID;

            SET IDENTITY_INSERT dbo.SystemSettings OFF;

            /*
             * Make the identity value continue after the existing data.
             */
            IF EXISTS
            (
                SELECT 1
                FROM dbo.SystemSettings
            )
            BEGIN
                DECLARE @MaxSystemSettingID int;

                SELECT
                    @MaxSystemSettingID = MAX(SystemSettingID)
                FROM dbo.SystemSettings;

                DBCC CHECKIDENT
                (
                    'dbo.SystemSettings',
                    RESEED,
                    @MaxSystemSettingID
                ) WITH NO_INFOMSGS;
            END;

            /*
             * Prevent duplicate setting keys.
             */
            CREATE UNIQUE INDEX UX_SystemSettings_Key
                ON dbo.SystemSettings (SettingKey);

            /*
             * Remove the temporary legacy table.
             */
            DROP TABLE dbo.SystemSettings_Legacy;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.SystemSettings', N'U') IS NULL
            BEGIN
                RETURN;
            END;

            IF OBJECT_ID(N'dbo.SystemSettings_Aligned', N'U') IS NOT NULL
            BEGIN
                DROP TABLE dbo.SystemSettings_Aligned;
            END;

            EXEC sp_rename
                N'dbo.SystemSettings',
                N'SystemSettings_Aligned';

            CREATE TABLE dbo.SystemSettings
            (
                SystemSettingID int IDENTITY(1,1) NOT NULL,
                SettingKey nvarchar(150) NOT NULL,
                SettingValue nvarchar(4000) NULL,
                Description nvarchar(500) NULL,
                IsEncrypted bit NOT NULL,
                IsActive bit NOT NULL,
                CreatedDate datetime2 NOT NULL,
                ModifiedDate datetime2 NULL,

                CONSTRAINT PK_SystemSettings
                    PRIMARY KEY (SystemSettingID)
            );

            SET IDENTITY_INSERT dbo.SystemSettings ON;

            INSERT INTO dbo.SystemSettings
            (
                SystemSettingID,
                SettingKey,
                SettingValue,
                Description,
                IsEncrypted,
                IsActive,
                CreatedDate,
                ModifiedDate
            )
            SELECT
                SystemSettingID,
                SettingKey,
                SettingValue,
                Description,
                IsEncrypted,
                IsActive,
                CreatedDate,
                ModifiedDate
            FROM dbo.SystemSettings_Aligned
            ORDER BY SystemSettingID;

            SET IDENTITY_INSERT dbo.SystemSettings OFF;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.SystemSettings
            )
            BEGIN
                DECLARE @MaxSystemSettingID int;

                SELECT
                    @MaxSystemSettingID = MAX(SystemSettingID)
                FROM dbo.SystemSettings;

                DBCC CHECKIDENT
                (
                    'dbo.SystemSettings',
                    RESEED,
                    @MaxSystemSettingID
                ) WITH NO_INFOMSGS;
            END;

            CREATE UNIQUE INDEX UX_SystemSettings_Key
                ON dbo.SystemSettings (SettingKey);

            DROP TABLE dbo.SystemSettings_Aligned;
            """);
    }
}