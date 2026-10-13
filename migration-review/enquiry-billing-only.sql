BEGIN TRANSACTION;
CREATE TABLE [EnquiryPackages] (
    [EnquiryPackageID] int NOT NULL IDENTITY,
    [PackageCode] varchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [MonthlySenderEnquiryLimit] int NOT NULL,
    [MonthlyAdvertisementEnquiryLimit] int NOT NULL,
    [Price] decimal(18,3) NOT NULL,
    [CurrencyCode] varchar(3) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_EnquiryPackages] PRIMARY KEY ([EnquiryPackageID]),
    CONSTRAINT [CK_EnquiryPackages_AdvertisementLimit] CHECK ([MonthlyAdvertisementEnquiryLimit] >= 0),
    CONSTRAINT [CK_EnquiryPackages_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_EnquiryPackages_SenderLimit] CHECK ([MonthlySenderEnquiryLimit] >= 0)
);

CREATE TABLE [UserPackageEntitlements] (
    [UserPackageEntitlementID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [EnquiryPackageID] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [Status] varchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_UserPackageEntitlements] PRIMARY KEY ([UserPackageEntitlementID]),
    CONSTRAINT [CK_UserPackageEntitlements_DateRange] CHECK ([EndDate] IS NULL OR [EndDate] > [StartDate]),
    CONSTRAINT [CK_UserPackageEntitlements_Status] CHECK ([Status] IN ('PENDING', 'ACTIVE', 'CANCELLED', 'EXPIRED')),
    CONSTRAINT [FK_UserPackageEntitlements_EnquiryPackages_EnquiryPackageID] FOREIGN KEY ([EnquiryPackageID]) REFERENCES [EnquiryPackages] ([EnquiryPackageID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserPackageEntitlements_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE TABLE [PaymentTransactions] (
    [PaymentTransactionID] bigint NOT NULL IDENTITY,
    [UserPackageEntitlementID] bigint NOT NULL,
    [Amount] decimal(18,3) NOT NULL,
    [CurrencyCode] varchar(3) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [PaymentProvider] nvarchar(50) NOT NULL,
    [ProviderTransactionReference] nvarchar(200) NULL,
    [Status] varchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ConfirmedDate] datetime2 NULL,
    [RefundedDate] datetime2 NULL,
    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([PaymentTransactionID]),
    CONSTRAINT [CK_PaymentTransactions_Amount] CHECK ([Amount] >= 0),
    CONSTRAINT [CK_PaymentTransactions_Status] CHECK ([Status] IN ('PENDING', 'CONFIRMED', 'FAILED', 'REFUNDED')),
    CONSTRAINT [FK_PaymentTransactions_UserPackageEntitlements_UserPackageEntitlementID] FOREIGN KEY ([UserPackageEntitlementID]) REFERENCES [UserPackageEntitlements] ([UserPackageEntitlementID]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_EnquiryPackages_Name] ON [EnquiryPackages] ([Name]);

CREATE UNIQUE INDEX [IX_EnquiryPackages_PackageCode] ON [EnquiryPackages] ([PackageCode]);

CREATE UNIQUE INDEX [IX_PaymentTransactions_PaymentProvider_ProviderTransactionReference] ON [PaymentTransactions] ([PaymentProvider], [ProviderTransactionReference]) WHERE [ProviderTransactionReference] IS NOT NULL;

CREATE INDEX [IX_PaymentTransactions_UserPackageEntitlementID] ON [PaymentTransactions] ([UserPackageEntitlementID]);

CREATE INDEX [IX_UserPackageEntitlements_EnquiryPackageID] ON [UserPackageEntitlements] ([EnquiryPackageID]);

CREATE UNIQUE INDEX [IX_UserPackageEntitlements_UserID] ON [UserPackageEntitlements] ([UserID]) WHERE [Status] = 'ACTIVE';

CREATE INDEX [IX_UserPackageEntitlements_UserID_Status_StartDate_EndDate] ON [UserPackageEntitlements] ([UserID], [Status], [StartDate], [EndDate]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261013084500_AddEnquiryBillingPackages', N'9.0.9');

COMMIT;
GO

