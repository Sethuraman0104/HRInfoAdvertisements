BEGIN TRANSACTION;

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


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261023091014_RemoveRedundantAdvertisementFeatureIDColumn', N'9.0.9');

COMMIT;
GO

