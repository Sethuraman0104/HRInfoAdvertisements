using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _CanonicalSchemaAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Canonical AdvertisementImages schema:
             *
             * AdvertisementImageID bigint
             * AdvertisementID      bigint
             * FileName             nvarchar(255) NOT NULL
             * FileURL              nvarchar(1000) NOT NULL
             * StorageKey           nvarchar(1000) NULL
             * ContentType          nvarchar(100) NULL
             * FileSize             bigint NULL
             * IsPrimary            bit NOT NULL
             * DisplayOrder         int NOT NULL
             * CreatedDate          datetime2 NOT NULL
             *
             * Legacy Home schema additionally contains:
             *   S3Key
             *   Width
             *   Height
             *
             * This migration is deliberately conditional because the
             * database may already contain the canonical schema.
             */

            migrationBuilder.Sql(
                """
                /* ============================================================
                   1. Add StorageKey if it does not exist
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'StorageKey') IS NULL
                BEGIN
                    ALTER TABLE dbo.AdvertisementImages
                    ADD StorageKey nvarchar(1000) NULL;
                END;


                /* ============================================================
                   2. Copy S3Key -> StorageKey when legacy S3Key exists
                      Dynamic SQL is required because S3Key may not exist.
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'S3Key') IS NOT NULL
                   AND COL_LENGTH(N'dbo.AdvertisementImages', N'StorageKey') IS NOT NULL
                BEGIN
                    EXEC sys.sp_executesql N'
                        UPDATE dbo.AdvertisementImages
                        SET StorageKey = S3Key
                        WHERE StorageKey IS NULL
                          AND S3Key IS NOT NULL;
                    ';
                END;


                /* ============================================================
                   3. Add ContentType if it does not exist
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'ContentType') IS NULL
                BEGIN
                    ALTER TABLE dbo.AdvertisementImages
                    ADD ContentType nvarchar(100) NULL;
                END;


                /* ============================================================
                   4. Remove Width if it exists
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'Width') IS NOT NULL
                BEGIN
                    EXEC sys.sp_executesql N'
                        ALTER TABLE dbo.AdvertisementImages
                        DROP COLUMN Width;
                    ';
                END;


                /* ============================================================
                   5. Remove Height if it exists
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'Height') IS NOT NULL
                BEGIN
                    EXEC sys.sp_executesql N'
                        ALTER TABLE dbo.AdvertisementImages
                        DROP COLUMN Height;
                    ';
                END;


                /* ============================================================
                   6. Remove S3Key if it exists

                      The data has already been copied into StorageKey.
                   ============================================================ */

                IF COL_LENGTH(N'dbo.AdvertisementImages', N'S3Key') IS NOT NULL
                BEGIN
                    EXEC sys.sp_executesql N'
                        ALTER TABLE dbo.AdvertisementImages
                        DROP COLUMN S3Key;
                    ';
                END;


                /* ============================================================
                   7. Normalize FileName

                      Legacy  = nvarchar(250)
                      Current = nvarchar(255)
                   ============================================================ */

                ALTER TABLE dbo.AdvertisementImages
                ALTER COLUMN FileName nvarchar(255) NOT NULL;


                /* ============================================================
                   8. Normalize FileURL

                      Legacy  = nvarchar(1500) NULL
                      Current = nvarchar(1000) NOT NULL

                      Home verification:
                      - 26 rows
                      - 0 missing FileURL
                      - maximum length = 88
                   ============================================================ */

                ALTER TABLE dbo.AdvertisementImages
                ALTER COLUMN FileURL nvarchar(1000) NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
             * This migration is a forward reconciliation migration.
             *
             * A destructive automatic rollback is intentionally not
             * provided because S3Key data has been migrated into
             * StorageKey and Width/Height are no longer part of the
             * canonical model.
             *
             * Restore from database backup if rollback is required.
             */
        }
    }
}