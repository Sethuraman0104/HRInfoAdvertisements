BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SystemSettings', N'U') IS NULL
BEGIN
    RETURN;
END;

IF OBJECT_ID(N'dbo.SystemSettings_Legacy', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.SystemSettings_Legacy;
END;

EXEC sp_rename
    N'dbo.SystemSettings',
    N'SystemSettings_Legacy';

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
    SettingID,
    CONVERT(nvarchar(150), SettingKey),
    CONVERT(nvarchar(4000), SettingValue),
    CONVERT(nvarchar(500), Description),
    DataType,
    IsEncrypted,
    IsActive,
    COALESCE(
        ModifiedDate,
        SYSUTCDATETIME()
    ),
    ModifiedDate
FROM dbo.SystemSettings_Legacy
ORDER BY SettingID;

SET IDENTITY_INSERT dbo.SystemSettings OFF;

IF EXISTS
(
    SELECT 1
    FROM dbo.SystemSettings
)
BEGIN
    DECLARE @MaxSystemSettingID int;

    SELECT
        @MaxSystemSettingID =
            MAX(SystemSettingID)
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

DROP TABLE dbo.SystemSettings_Legacy;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930171053_AlignSystemSettingsSchema', N'9.0.9');

COMMIT;
GO

