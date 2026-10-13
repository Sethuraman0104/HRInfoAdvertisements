BEGIN TRANSACTION;
CREATE TABLE [PackagePurchaseRequests] (
    [PackagePurchaseRequestID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [EnquiryPackageID] int NOT NULL,
    [Amount] decimal(18,3) NOT NULL,
    [CurrencyCode] varchar(3) NOT NULL,
    [Status] varchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_PackagePurchaseRequests] PRIMARY KEY ([PackagePurchaseRequestID]),
    CONSTRAINT [CK_PackagePurchaseRequests_Amount] CHECK ([Amount] >= 0),
    CONSTRAINT [CK_PackagePurchaseRequests_Status] CHECK ([Status] IN ('PENDING', 'APPROVED', 'COMPLETED', 'REJECTED', 'CANCELLED')),
    CONSTRAINT [FK_PackagePurchaseRequests_EnquiryPackages_EnquiryPackageID] FOREIGN KEY ([EnquiryPackageID]) REFERENCES [EnquiryPackages] ([EnquiryPackageID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PackagePurchaseRequests_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_PackagePurchaseRequests_EnquiryPackageID] ON [PackagePurchaseRequests] ([EnquiryPackageID]);

CREATE UNIQUE INDEX [IX_PackagePurchaseRequests_UserID] ON [PackagePurchaseRequests] ([UserID]) WHERE [Status] = 'PENDING';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261013164827_AddPackagePurchaseRequests', N'9.0.9');

COMMIT;
GO

