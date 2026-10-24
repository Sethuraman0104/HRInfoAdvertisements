BEGIN TRANSACTION;
ALTER TABLE [AdvertisementFeatures] ADD [CategoryID] int NULL;

ALTER TABLE [AdvertisementFeatures] ADD [IsRequired] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AdvertisementFeatures] ADD [HelpText] nvarchar(500) NULL;

ALTER TABLE [AdvertisementFeatures] ADD [HelpTextAr] nvarchar(500) NULL;

ALTER TABLE [AdvertisementFeatures] ADD [IsFilterable] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AdvertisementFeatures] ADD [IsSearchable] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AdvertisementFeatures] ADD [MaxLength] int NULL;

ALTER TABLE [AdvertisementFeatures] ADD [MaxValue] decimal(18,4) NULL;

ALTER TABLE [AdvertisementFeatures] ADD [MinLength] int NULL;

ALTER TABLE [AdvertisementFeatures] ADD [MinValue] decimal(18,4) NULL;

ALTER TABLE [AdvertisementFeatures] ADD [ValidationPattern] nvarchar(1000) NULL;

ALTER TABLE [AdvertisementCategories] ADD [ParentCategoryID] int NULL;

CREATE TABLE [AdvertisementFeatureOptions] (
    [AdvertisementFeatureOptionID] int NOT NULL IDENTITY,
    [AdvertisementFeatureID] int NOT NULL,
    [OptionValue] nvarchar(150) NOT NULL,
    [OptionValueAr] nvarchar(150) NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_AdvertisementFeatureOptions] PRIMARY KEY ([AdvertisementFeatureOptionID]),
    CONSTRAINT [FK_AdvertisementFeatureOptions_AdvertisementFeatures_AdvertisementFeatureID] FOREIGN KEY ([AdvertisementFeatureID]) REFERENCES [AdvertisementFeatures] ([FeatureID]) ON DELETE CASCADE
);

CREATE INDEX [IX_AdvertisementCategories_ParentCategoryID] ON [AdvertisementCategories] ([ParentCategoryID]);

CREATE INDEX [IX_AdvertisementFeatures_CategoryID] ON [AdvertisementFeatures] ([CategoryID]);

ALTER TABLE [AdvertisementFeatures] ADD CONSTRAINT [FK_AdvertisementFeatures_AdvertisementCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [AdvertisementCategories] ([CategoryID]) ON DELETE NO ACTION;

CREATE UNIQUE INDEX [IX_AdvertisementFeatureOptions_AdvertisementFeatureID_OptionValue] ON [AdvertisementFeatureOptions] ([AdvertisementFeatureID], [OptionValue]);

ALTER TABLE [AdvertisementCategories] ADD CONSTRAINT [FK_AdvertisementCategories_AdvertisementCategories_ParentCategoryID] FOREIGN KEY ([ParentCategoryID]) REFERENCES [AdvertisementCategories] ([CategoryID]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261022165301_AddAdvertisementFeatureOptionsAndValidationFields', N'9.0.9');

COMMIT;
GO

