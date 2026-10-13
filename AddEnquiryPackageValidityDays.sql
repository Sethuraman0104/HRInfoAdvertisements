BEGIN TRANSACTION;
ALTER TABLE [EnquiryPackages] ADD [ValidityDays] int NULL;

ALTER TABLE [EnquiryPackages] ADD CONSTRAINT [CK_EnquiryPackages_ValidityDays] CHECK ([ValidityDays] IS NULL OR [ValidityDays] > 0);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261013141739_AddEnquiryPackageValidityDays', N'9.0.9');

COMMIT;
GO

