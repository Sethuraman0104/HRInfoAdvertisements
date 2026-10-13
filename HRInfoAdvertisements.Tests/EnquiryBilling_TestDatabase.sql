IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AdvertisementCategories] (
    [CategoryID] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(150) NOT NULL,
    [CategoryNameAr] nvarchar(150) NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_AdvertisementCategories] PRIMARY KEY ([CategoryID])
);

CREATE TABLE [AdvertisementStatuses] (
    [StatusID] int NOT NULL IDENTITY,
    [StatusCode] nvarchar(50) NOT NULL,
    [StatusName] nvarchar(100) NOT NULL,
    [StatusNameAr] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [IsPublicStatus] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_AdvertisementStatuses] PRIMARY KEY ([StatusID])
);

CREATE TABLE [AdvertisementTypes] (
    [AdvertisementTypeID] int NOT NULL IDENTITY,
    [TypeName] nvarchar(100) NOT NULL,
    [TypeNameAr] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_AdvertisementTypes] PRIMARY KEY ([AdvertisementTypeID])
);

CREATE TABLE [Countries] (
    [CountryID] int NOT NULL IDENTITY,
    [CountryCode] nvarchar(10) NOT NULL,
    [CountryName] nvarchar(150) NOT NULL,
    [CountryNameAr] nvarchar(150) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryID])
);

CREATE TABLE [Locations] (
    [LocationID] bigint NOT NULL IDENTITY,
    [LocationName] nvarchar(max) NOT NULL,
    [LocationNameAr] nvarchar(max) NULL,
    [Latitude] decimal(18,2) NULL,
    [Longitude] decimal(18,2) NULL,
    [AddressLine] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationID])
);

CREATE TABLE [NotificationTemplates] (
    [NotificationTemplateID] int NOT NULL IDENTITY,
    [TemplateCode] nvarchar(100) NOT NULL,
    [TemplateName] nvarchar(150) NOT NULL,
    [Subject] nvarchar(max) NULL,
    [Body] nvarchar(max) NULL,
    [SubjectAr] nvarchar(max) NULL,
    [BodyAr] nvarchar(max) NULL,
    [IsEmailEnabled] bit NOT NULL,
    [IsSmsEnabled] bit NOT NULL,
    [IsPushEnabled] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY ([NotificationTemplateID])
);

CREATE TABLE [Permissions] (
    [PermissionID] int NOT NULL IDENTITY,
    [PermissionCode] nvarchar(100) NOT NULL,
    [PermissionName] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([PermissionID])
);

CREATE TABLE [RejectionReasons] (
    [RejectionReasonID] int NOT NULL IDENTITY,
    [ReasonCode] nvarchar(max) NOT NULL,
    [ReasonText] nvarchar(max) NOT NULL,
    [ReasonTextAr] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_RejectionReasons] PRIMARY KEY ([RejectionReasonID])
);

CREATE TABLE [ReportReasons] (
    [ReportReasonID] int NOT NULL IDENTITY,
    [ReasonCode] nvarchar(50) NOT NULL,
    [ReasonText] nvarchar(300) NOT NULL,
    [ReasonTextAr] nvarchar(300) NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_ReportReasons] PRIMARY KEY ([ReportReasonID])
);

CREATE TABLE [Roles] (
    [RoleID] int NOT NULL IDENTITY,
    [RoleName] nvarchar(max) NOT NULL,
    [RoleDescription] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleID])
);

CREATE TABLE [SystemSettings] (
    [SystemSettingID] int NOT NULL IDENTITY,
    [SettingKey] nvarchar(150) NOT NULL,
    [SettingValue] nvarchar(4000) NULL,
    [Description] nvarchar(500) NULL,
    [IsEncrypted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SystemSettingID])
);

CREATE TABLE [Users] (
    [UserID] bigint NOT NULL IDENTITY,
    [UserName] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [MobileNo] nvarchar(30) NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [IsEmailVerified] bit NOT NULL,
    [IsMobileVerified] bit NOT NULL,
    [IsMFAEnabled] bit NOT NULL,
    [AccountStatus] nvarchar(30) NOT NULL,
    [FailedLoginAttempts] int NOT NULL,
    [LockoutEndDate] datetime2 NULL,
    [LastLoginDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [CreatedBy] bigint NULL,
    [ModifiedBy] bigint NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserID])
);

CREATE TABLE [AdvertisementFeatures] (
    [AdvertisementFeatureID] int NOT NULL IDENTITY,
    [CategoryID] int NOT NULL,
    [FeatureName] nvarchar(150) NOT NULL,
    [FeatureNameAr] nvarchar(150) NULL,
    [DataType] nvarchar(30) NOT NULL,
    [IsRequired] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_AdvertisementFeatures] PRIMARY KEY ([AdvertisementFeatureID]),
    CONSTRAINT [FK_AdvertisementFeatures_AdvertisementCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [AdvertisementCategories] ([CategoryID]) ON DELETE CASCADE
);

CREATE TABLE [States] (
    [StateID] int NOT NULL IDENTITY,
    [CountryID] int NOT NULL,
    [StateName] nvarchar(150) NOT NULL,
    [StateNameAr] nvarchar(150) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_States] PRIMARY KEY ([StateID]),
    CONSTRAINT [FK_States_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]) ON DELETE NO ACTION
);

CREATE TABLE [RolePermissions] (
    [RolePermissionID] bigint NOT NULL IDENTITY,
    [RoleID] int NOT NULL,
    [PermissionID] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RolePermissionID]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionID] FOREIGN KEY ([PermissionID]) REFERENCES [Permissions] ([PermissionID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RolePermissions_Roles_RoleID] FOREIGN KEY ([RoleID]) REFERENCES [Roles] ([RoleID]) ON DELETE CASCADE
);

CREATE TABLE [AuditLogs] (
    [AuditLogID] bigint NOT NULL IDENTITY,
    [UserID] bigint NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(150) NOT NULL,
    [EntityID] nvarchar(100) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IPAddress] nvarchar(100) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogID]),
    CONSTRAINT [FK_AuditLogs_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
);

CREATE TABLE [LoginHistories] (
    [LoginHistoryID] bigint NOT NULL IDENTITY,
    [UserID] bigint NULL,
    [LoginIdentifier] nvarchar(255) NOT NULL,
    [IsSuccessful] bit NOT NULL,
    [FailureReason] nvarchar(500) NULL,
    [IPAddress] nvarchar(100) NULL,
    [UserAgent] nvarchar(1000) NULL,
    [LoginDate] datetime2 NOT NULL,
    CONSTRAINT [PK_LoginHistories] PRIMARY KEY ([LoginHistoryID]),
    CONSTRAINT [FK_LoginHistories_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
);

CREATE TABLE [Notifications] (
    [NotificationID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [NotificationTemplateID] int NULL,
    [NotificationType] nvarchar(50) NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [Message] nvarchar(2000) NULL,
    [ReferenceType] nvarchar(100) NULL,
    [ReferenceID] bigint NULL,
    [IsRead] bit NOT NULL,
    [ReadDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID]),
    CONSTRAINT [FK_Notifications_NotificationTemplates_NotificationTemplateID] FOREIGN KEY ([NotificationTemplateID]) REFERENCES [NotificationTemplates] ([NotificationTemplateID]) ON DELETE SET NULL,
    CONSTRAINT [FK_Notifications_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [OTPRequests] (
    [OTPRequestID] bigint NOT NULL IDENTITY,
    [UserID] bigint NULL,
    [Destination] nvarchar(255) NOT NULL,
    [OTPHash] nvarchar(500) NOT NULL,
    [Purpose] nvarchar(50) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [AttemptCount] int NOT NULL,
    [MaxAttempts] int NOT NULL,
    [IsConsumed] bit NOT NULL,
    [ConsumedDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_OTPRequests] PRIMARY KEY ([OTPRequestID]),
    CONSTRAINT [FK_OTPRequests_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
);

CREATE TABLE [RefreshTokens] (
    [RefreshTokenID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [TokenHash] nvarchar(500) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    [ReplacedByTokenHash] nvarchar(500) NULL,
    [CreatedByIPAddress] nvarchar(100) NULL,
    [RevokedByIPAddress] nvarchar(100) NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([RefreshTokenID]),
    CONSTRAINT [FK_RefreshTokens_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [UserProfiles] (
    [UserProfileID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NULL,
    [ProfilePhotoURL] nvarchar(1000) NULL,
    [Nationality] nvarchar(100) NULL,
    [PreferredLanguage] nvarchar(10) NOT NULL DEFAULT N'en',
    [IsBusinessAccount] bit NOT NULL,
    [CompanyName] nvarchar(250) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([UserProfileID]),
    CONSTRAINT [FK_UserProfiles_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [UserRoles] (
    [UserRoleID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [RoleID] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleID]),
    CONSTRAINT [FK_UserRoles_Roles_RoleID] FOREIGN KEY ([RoleID]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserRoles_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [UserSessions] (
    [UserSessionID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [SessionTokenHash] nvarchar(500) NOT NULL,
    [DeviceName] nvarchar(200) NULL,
    [DeviceType] nvarchar(50) NULL,
    [IPAddress] nvarchar(100) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [LastActivityDate] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    [RevokedDate] datetime2 NULL,
    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([UserSessionID]),
    CONSTRAINT [FK_UserSessions_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [Cities] (
    [CityID] int NOT NULL IDENTITY,
    [CountryID] int NOT NULL,
    [StateID] int NULL,
    [CityName] nvarchar(150) NOT NULL,
    [CityNameAr] nvarchar(150) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([CityID]),
    CONSTRAINT [FK_Cities_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Cities_States_StateID] FOREIGN KEY ([StateID]) REFERENCES [States] ([StateID]) ON DELETE NO ACTION
);

CREATE TABLE [Areas] (
    [AreaID] int NOT NULL IDENTITY,
    [CountryID] int NOT NULL,
    [StateID] int NULL,
    [CityID] int NULL,
    [AreaName] nvarchar(150) NOT NULL,
    [AreaNameAr] nvarchar(150) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Areas] PRIMARY KEY ([AreaID]),
    CONSTRAINT [FK_Areas_Cities_CityID] FOREIGN KEY ([CityID]) REFERENCES [Cities] ([CityID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Areas_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Areas_States_StateID] FOREIGN KEY ([StateID]) REFERENCES [States] ([StateID]) ON DELETE NO ACTION
);

CREATE TABLE [Advertisements] (
    [AdvertisementID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [CategoryID] int NOT NULL,
    [AdvertisementTypeID] int NOT NULL,
    [StatusID] int NOT NULL,
    [AdvertisementNumber] nvarchar(50) NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [TitleAr] nvarchar(250) NULL,
    [Description] nvarchar(max) NOT NULL,
    [DescriptionAr] nvarchar(max) NULL,
    [Price] decimal(18,3) NULL,
    [CurrencyCode] nvarchar(10) NOT NULL DEFAULT N'BHD',
    [IsNegotiable] bit NOT NULL,
    [CountryID] int NULL,
    [StateID] int NULL,
    [CityID] int NULL,
    [AreaID] int NULL,
    [AddressLine] nvarchar(max) NULL,
    [Latitude] decimal(10,7) NULL,
    [Longitude] decimal(10,7) NULL,
    [PlotNumber] nvarchar(max) NULL,
    [LandArea] decimal(18,2) NULL,
    [BuiltUpArea] decimal(18,2) NULL,
    [Bedrooms] int NULL,
    [Bathrooms] int NULL,
    [PropertyAge] int NULL,
    [IsFeatured] bit NOT NULL,
    [FeaturedUntil] datetime2 NULL,
    [PublishedDate] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [CreatedBy] bigint NULL,
    [ModifiedBy] bigint NULL,
    CONSTRAINT [PK_Advertisements] PRIMARY KEY ([AdvertisementID]),
    CONSTRAINT [FK_Advertisements_AdvertisementCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [AdvertisementCategories] ([CategoryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_AdvertisementStatuses_StatusID] FOREIGN KEY ([StatusID]) REFERENCES [AdvertisementStatuses] ([StatusID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_AdvertisementTypes_AdvertisementTypeID] FOREIGN KEY ([AdvertisementTypeID]) REFERENCES [AdvertisementTypes] ([AdvertisementTypeID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_Areas_AreaID] FOREIGN KEY ([AreaID]) REFERENCES [Areas] ([AreaID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_Cities_CityID] FOREIGN KEY ([CityID]) REFERENCES [Cities] ([CityID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_States_StateID] FOREIGN KEY ([StateID]) REFERENCES [States] ([StateID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Advertisements_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE TABLE [UserAddresses] (
    [UserAddressID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [CountryID] int NULL,
    [StateID] int NULL,
    [CityID] int NULL,
    [AreaID] int NULL,
    [AddressLine1] nvarchar(300) NULL,
    [AddressLine2] nvarchar(300) NULL,
    [BuildingNo] nvarchar(50) NULL,
    [RoadNo] nvarchar(50) NULL,
    [BlockNo] nvarchar(50) NULL,
    [PostalCode] nvarchar(30) NULL,
    [IsPrimary] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([UserAddressID]),
    CONSTRAINT [FK_UserAddresses_Areas_AreaID] FOREIGN KEY ([AreaID]) REFERENCES [Areas] ([AreaID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAddresses_Cities_CityID] FOREIGN KEY ([CityID]) REFERENCES [Cities] ([CityID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAddresses_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAddresses_States_StateID] FOREIGN KEY ([StateID]) REFERENCES [States] ([StateID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAddresses_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [AdvertisementDocuments] (
    [AdvertisementDocumentID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [DocumentName] nvarchar(255) NOT NULL,
    [FileURL] nvarchar(1000) NOT NULL,
    [StorageKey] nvarchar(1000) NULL,
    [ContentType] nvarchar(100) NULL,
    [FileSize] bigint NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementDocuments] PRIMARY KEY ([AdvertisementDocumentID]),
    CONSTRAINT [FK_AdvertisementDocuments_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE
);

CREATE TABLE [AdvertisementEnquiries] (
    [AdvertisementEnquiryID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [SenderUserID] bigint NOT NULL,
    [Message] nvarchar(3000) NOT NULL,
    [ContactMobile] nvarchar(30) NULL,
    [ContactEmail] nvarchar(255) NULL,
    [Status] nvarchar(30) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [RespondedDate] datetime2 NULL,
    CONSTRAINT [PK_AdvertisementEnquiries] PRIMARY KEY ([AdvertisementEnquiryID]),
    CONSTRAINT [FK_AdvertisementEnquiries_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AdvertisementEnquiries_Users_SenderUserID] FOREIGN KEY ([SenderUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE TABLE [AdvertisementFeatureValues] (
    [AdvertisementFeatureValueID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [AdvertisementFeatureID] int NOT NULL,
    [FeatureValue] nvarchar(1000) NULL,
    [FeatureValueAr] nvarchar(1000) NULL,
    CONSTRAINT [PK_AdvertisementFeatureValues] PRIMARY KEY ([AdvertisementFeatureValueID]),
    CONSTRAINT [FK_AdvertisementFeatureValues_AdvertisementFeatures_AdvertisementFeatureID] FOREIGN KEY ([AdvertisementFeatureID]) REFERENCES [AdvertisementFeatures] ([AdvertisementFeatureID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AdvertisementFeatureValues_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE
);

CREATE TABLE [AdvertisementImages] (
    [AdvertisementImageID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FileURL] nvarchar(1000) NOT NULL,
    [StorageKey] nvarchar(1000) NULL,
    [ContentType] nvarchar(100) NULL,
    [FileSize] bigint NULL,
    [IsPrimary] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementImages] PRIMARY KEY ([AdvertisementImageID]),
    CONSTRAINT [FK_AdvertisementImages_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE
);

CREATE TABLE [AdvertisementVideos] (
    [AdvertisementVideoID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FileURL] nvarchar(1000) NOT NULL,
    [StorageKey] nvarchar(1000) NULL,
    [ContentType] nvarchar(100) NULL,
    [FileSize] bigint NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementVideos] PRIMARY KEY ([AdvertisementVideoID]),
    CONSTRAINT [FK_AdvertisementVideos_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE
);

CREATE TABLE [AdvertisementViews] (
    [AdvertisementViewID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [UserID] bigint NULL,
    [IPAddress] nvarchar(100) NULL,
    [UserAgent] nvarchar(1000) NULL,
    [ViewedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementViews] PRIMARY KEY ([AdvertisementViewID]),
    CONSTRAINT [FK_AdvertisementViews_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AdvertisementViews_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
);

CREATE TABLE [ApprovalRequests] (
    [ApprovalRequestID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [SubmittedByUserID] bigint NOT NULL,
    [AssignedToUserID] bigint NULL,
    [Status] nvarchar(30) NOT NULL,
    [Comments] nvarchar(2000) NULL,
    [SubmittedDate] datetime2 NOT NULL,
    [CompletedDate] datetime2 NULL,
    CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([ApprovalRequestID]),
    CONSTRAINT [FK_ApprovalRequests_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApprovalRequests_Users_AssignedToUserID] FOREIGN KEY ([AssignedToUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
    CONSTRAINT [FK_ApprovalRequests_Users_SubmittedByUserID] FOREIGN KEY ([SubmittedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE TABLE [Favorites] (
    [FavoriteID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [AdvertisementID] bigint NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([FavoriteID]),
    CONSTRAINT [FK_Favorites_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Favorites_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [Reports] (
    [ReportID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [ReportedByUserID] bigint NOT NULL,
    [ReportReasonID] int NOT NULL,
    [Comments] nvarchar(2000) NULL,
    [Status] nvarchar(30) NOT NULL,
    [ReviewedByUserID] bigint NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ReviewedDate] datetime2 NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportID]),
    CONSTRAINT [FK_Reports_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reports_ReportReasons_ReportReasonID] FOREIGN KEY ([ReportReasonID]) REFERENCES [ReportReasons] ([ReportReasonID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reports_Users_ReportedByUserID] FOREIGN KEY ([ReportedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reports_Users_ReviewedByUserID] FOREIGN KEY ([ReviewedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
);

CREATE TABLE [SavedSearches] (
    [SavedSearchID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [SearchName] nvarchar(150) NOT NULL,
    [SearchCriteriaJson] nvarchar(max) NULL,
    [EnableNotifications] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [AdvertisementID] bigint NULL,
    CONSTRAINT [PK_SavedSearches] PRIMARY KEY ([SavedSearchID]),
    CONSTRAINT [FK_SavedSearches_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]),
    CONSTRAINT [FK_SavedSearches_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE TABLE [Messages] (
    [MessageID] bigint NOT NULL IDENTITY,
    [SenderUserID] bigint NOT NULL,
    [ReceiverUserID] bigint NOT NULL,
    [AdvertisementID] bigint NOT NULL,
    [EnquiryID] bigint NULL,
    [MessageText] nvarchar(4000) NOT NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ReadDate] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageID]),
    CONSTRAINT [FK_Messages_AdvertisementEnquiries_EnquiryID] FOREIGN KEY ([EnquiryID]) REFERENCES [AdvertisementEnquiries] ([AdvertisementEnquiryID]),
    CONSTRAINT [FK_Messages_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]),
    CONSTRAINT [FK_Messages_Users_ReceiverUserID] FOREIGN KEY ([ReceiverUserID]) REFERENCES [Users] ([UserID]),
    CONSTRAINT [FK_Messages_Users_SenderUserID] FOREIGN KEY ([SenderUserID]) REFERENCES [Users] ([UserID])
);

CREATE TABLE [ApprovalHistories] (
    [ApprovalHistoryID] bigint NOT NULL IDENTITY,
    [ApprovalRequestID] bigint NOT NULL,
    [ActionedByUserID] bigint NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [Comments] nvarchar(2000) NULL,
    [ActionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY ([ApprovalHistoryID]),
    CONSTRAINT [FK_ApprovalHistories_ApprovalRequests_ApprovalRequestID] FOREIGN KEY ([ApprovalRequestID]) REFERENCES [ApprovalRequests] ([ApprovalRequestID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApprovalHistories_Users_ActionedByUserID] FOREIGN KEY ([ActionedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_AdvertisementCategories_CategoryName] ON [AdvertisementCategories] ([CategoryName]);

CREATE INDEX [IX_AdvertisementDocuments_AdvertisementID] ON [AdvertisementDocuments] ([AdvertisementID]);

CREATE INDEX [IX_AdvertisementEnquiries_AdvertisementID_CreatedDate] ON [AdvertisementEnquiries] ([AdvertisementID], [CreatedDate]);

CREATE INDEX [IX_AdvertisementEnquiries_SenderUserID] ON [AdvertisementEnquiries] ([SenderUserID]);

CREATE INDEX [IX_AdvertisementFeatures_CategoryID] ON [AdvertisementFeatures] ([CategoryID]);

CREATE INDEX [IX_AdvertisementFeatureValues_AdvertisementFeatureID] ON [AdvertisementFeatureValues] ([AdvertisementFeatureID]);

CREATE UNIQUE INDEX [IX_AdvertisementFeatureValues_AdvertisementID_AdvertisementFeatureID] ON [AdvertisementFeatureValues] ([AdvertisementID], [AdvertisementFeatureID]);

CREATE INDEX [IX_AdvertisementImages_AdvertisementID_DisplayOrder] ON [AdvertisementImages] ([AdvertisementID], [DisplayOrder]);

CREATE UNIQUE INDEX [IX_Advertisements_AdvertisementNumber] ON [Advertisements] ([AdvertisementNumber]);

CREATE INDEX [IX_Advertisements_AdvertisementTypeID] ON [Advertisements] ([AdvertisementTypeID]);

CREATE INDEX [IX_Advertisements_AreaID] ON [Advertisements] ([AreaID]);

CREATE INDEX [IX_Advertisements_CategoryID] ON [Advertisements] ([CategoryID]);

CREATE INDEX [IX_Advertisements_CityID] ON [Advertisements] ([CityID]);

CREATE INDEX [IX_Advertisements_CountryID] ON [Advertisements] ([CountryID]);

CREATE INDEX [IX_Advertisements_ExpiryDate] ON [Advertisements] ([ExpiryDate]);

CREATE INDEX [IX_Advertisements_Price] ON [Advertisements] ([Price]);

CREATE INDEX [IX_Advertisements_PublishedDate] ON [Advertisements] ([PublishedDate]);

CREATE INDEX [IX_Advertisements_StateID] ON [Advertisements] ([StateID]);

CREATE INDEX [IX_Advertisements_StatusID] ON [Advertisements] ([StatusID]);

CREATE INDEX [IX_Advertisements_UserID] ON [Advertisements] ([UserID]);

CREATE UNIQUE INDEX [IX_AdvertisementStatuses_StatusCode] ON [AdvertisementStatuses] ([StatusCode]);

CREATE UNIQUE INDEX [IX_AdvertisementTypes_TypeName] ON [AdvertisementTypes] ([TypeName]);

CREATE INDEX [IX_AdvertisementVideos_AdvertisementID] ON [AdvertisementVideos] ([AdvertisementID]);

CREATE INDEX [IX_AdvertisementViews_AdvertisementID_ViewedDate] ON [AdvertisementViews] ([AdvertisementID], [ViewedDate]);

CREATE INDEX [IX_AdvertisementViews_UserID] ON [AdvertisementViews] ([UserID]);

CREATE INDEX [IX_ApprovalHistories_ActionedByUserID] ON [ApprovalHistories] ([ActionedByUserID]);

CREATE INDEX [IX_ApprovalHistories_ApprovalRequestID] ON [ApprovalHistories] ([ApprovalRequestID]);

CREATE INDEX [IX_ApprovalRequests_AdvertisementID] ON [ApprovalRequests] ([AdvertisementID]);

CREATE INDEX [IX_ApprovalRequests_AssignedToUserID] ON [ApprovalRequests] ([AssignedToUserID]);

CREATE INDEX [IX_ApprovalRequests_Status_SubmittedDate] ON [ApprovalRequests] ([Status], [SubmittedDate]);

CREATE INDEX [IX_ApprovalRequests_SubmittedByUserID] ON [ApprovalRequests] ([SubmittedByUserID]);

CREATE INDEX [IX_Areas_CityID] ON [Areas] ([CityID]);

CREATE UNIQUE INDEX [IX_Areas_CountryID_StateID_CityID_AreaName] ON [Areas] ([CountryID], [StateID], [CityID], [AreaName]) WHERE [StateID] IS NOT NULL AND [CityID] IS NOT NULL;

CREATE INDEX [IX_Areas_StateID] ON [Areas] ([StateID]);

CREATE INDEX [IX_AuditLogs_CreatedDate] ON [AuditLogs] ([CreatedDate]);

CREATE INDEX [IX_AuditLogs_EntityName_EntityID] ON [AuditLogs] ([EntityName], [EntityID]);

CREATE INDEX [IX_AuditLogs_UserID] ON [AuditLogs] ([UserID]);

CREATE UNIQUE INDEX [IX_Cities_CountryID_StateID_CityName] ON [Cities] ([CountryID], [StateID], [CityName]) WHERE [StateID] IS NOT NULL;

CREATE INDEX [IX_Cities_StateID] ON [Cities] ([StateID]);

CREATE UNIQUE INDEX [IX_Countries_CountryCode] ON [Countries] ([CountryCode]);

CREATE UNIQUE INDEX [IX_Countries_CountryName] ON [Countries] ([CountryName]);

CREATE INDEX [IX_Favorites_AdvertisementID] ON [Favorites] ([AdvertisementID]);

CREATE UNIQUE INDEX [IX_Favorites_UserID_AdvertisementID] ON [Favorites] ([UserID], [AdvertisementID]);

CREATE INDEX [IX_LoginHistories_LoginDate] ON [LoginHistories] ([LoginDate]);

CREATE INDEX [IX_LoginHistories_UserID] ON [LoginHistories] ([UserID]);

CREATE INDEX [IX_Messages_AdvertisementID] ON [Messages] ([AdvertisementID]);

CREATE INDEX [IX_Messages_EnquiryID] ON [Messages] ([EnquiryID]);

CREATE INDEX [IX_Messages_ReceiverUserID_IsRead_CreatedDate] ON [Messages] ([ReceiverUserID], [IsRead], [CreatedDate]);

CREATE INDEX [IX_Messages_SenderUserID] ON [Messages] ([SenderUserID]);

CREATE INDEX [IX_Notifications_NotificationTemplateID] ON [Notifications] ([NotificationTemplateID]);

CREATE INDEX [IX_Notifications_UserID_IsRead_CreatedDate] ON [Notifications] ([UserID], [IsRead], [CreatedDate]);

CREATE UNIQUE INDEX [IX_NotificationTemplates_TemplateCode] ON [NotificationTemplates] ([TemplateCode]);

CREATE INDEX [IX_OTPRequests_Destination_Purpose_CreatedDate] ON [OTPRequests] ([Destination], [Purpose], [CreatedDate]);

CREATE INDEX [IX_OTPRequests_UserID] ON [OTPRequests] ([UserID]);

CREATE UNIQUE INDEX [IX_Permissions_PermissionCode] ON [Permissions] ([PermissionCode]);

CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);

CREATE INDEX [IX_RefreshTokens_UserID_ExpiresAt] ON [RefreshTokens] ([UserID], [ExpiresAt]);

CREATE UNIQUE INDEX [IX_ReportReasons_ReasonCode] ON [ReportReasons] ([ReasonCode]);

CREATE INDEX [IX_Reports_AdvertisementID] ON [Reports] ([AdvertisementID]);

CREATE INDEX [IX_Reports_ReportedByUserID] ON [Reports] ([ReportedByUserID]);

CREATE INDEX [IX_Reports_ReportReasonID] ON [Reports] ([ReportReasonID]);

CREATE INDEX [IX_Reports_ReviewedByUserID] ON [Reports] ([ReviewedByUserID]);

CREATE INDEX [IX_Reports_Status_CreatedDate] ON [Reports] ([Status], [CreatedDate]);

CREATE INDEX [IX_RolePermissions_PermissionID] ON [RolePermissions] ([PermissionID]);

CREATE UNIQUE INDEX [IX_RolePermissions_RoleID_PermissionID] ON [RolePermissions] ([RoleID], [PermissionID]);

CREATE INDEX [IX_SavedSearches_AdvertisementID] ON [SavedSearches] ([AdvertisementID]);

CREATE INDEX [IX_SavedSearches_UserID] ON [SavedSearches] ([UserID]);

CREATE UNIQUE INDEX [IX_States_CountryID_StateName] ON [States] ([CountryID], [StateName]);

CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);

CREATE INDEX [IX_UserAddresses_AreaID] ON [UserAddresses] ([AreaID]);

CREATE INDEX [IX_UserAddresses_CityID] ON [UserAddresses] ([CityID]);

CREATE INDEX [IX_UserAddresses_CountryID] ON [UserAddresses] ([CountryID]);

CREATE INDEX [IX_UserAddresses_StateID] ON [UserAddresses] ([StateID]);

CREATE INDEX [IX_UserAddresses_UserID] ON [UserAddresses] ([UserID]);

CREATE UNIQUE INDEX [IX_UserProfiles_UserID] ON [UserProfiles] ([UserID]);

CREATE INDEX [IX_UserRoles_RoleID] ON [UserRoles] ([RoleID]);

CREATE UNIQUE INDEX [IX_UserRoles_UserID_RoleID] ON [UserRoles] ([UserID], [RoleID]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Users_MobileNo] ON [Users] ([MobileNo]) WHERE [MobileNo] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);

CREATE UNIQUE INDEX [IX_UserSessions_SessionTokenHash] ON [UserSessions] ([SessionTokenHash]);

CREATE INDEX [IX_UserSessions_UserID] ON [UserSessions] ([UserID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922104447_InitialCreate', N'9.0.9');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'Longitude');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Locations] ALTER COLUMN [Longitude] decimal(10,7) NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'Latitude');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Locations] ALTER COLUMN [Latitude] decimal(10,7) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922105539_UpdateModel', N'9.0.9');

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Roles]') AND [c].[name] = N'RoleName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Roles] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Roles] ALTER COLUMN [RoleName] nvarchar(50) NOT NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Roles]') AND [c].[name] = N'RoleDescription');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Roles] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Roles] ALTER COLUMN [RoleDescription] nvarchar(500) NULL;

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923044031_AddAreaRelationships', N'9.0.9');

CREATE TABLE [AdvertisementFavorites] (
    [AdvertisementFavoriteID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [AdvertisementID] bigint NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementFavorites] PRIMARY KEY ([AdvertisementFavoriteID]),
    CONSTRAINT [FK_AdvertisementFavorites_Advertisements_AdvertisementID] FOREIGN KEY ([AdvertisementID]) REFERENCES [Advertisements] ([AdvertisementID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AdvertisementFavorites_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE INDEX [IX_AdvertisementFavorites_AdvertisementID] ON [AdvertisementFavorites] ([AdvertisementID]);

CREATE UNIQUE INDEX [IX_AdvertisementFavorites_UserID_AdvertisementID] ON [AdvertisementFavorites] ([UserID], [AdvertisementID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923053855_AddAdvertisementFavorites', N'9.0.9');

EXEC sp_rename N'[AdvertisementEnquiries].[RespondedDate]', N'LastRepliedDate', 'COLUMN';

ALTER TABLE [AdvertisementEnquiries] ADD [ClosedDate] datetime2 NULL;

ALTER TABLE [AdvertisementEnquiries] ADD [RecipientUserID] bigint NOT NULL DEFAULT CAST(0 AS bigint);

ALTER TABLE [AdvertisementEnquiries] ADD [Subject] nvarchar(max) NOT NULL DEFAULT N'';

CREATE TABLE [AdvertisementEnquiryMessages] (
    [AdvertisementEnquiryMessageID] bigint NOT NULL IDENTITY,
    [AdvertisementEnquiryID] bigint NOT NULL,
    [SenderUserID] bigint NOT NULL,
    [Message] nvarchar(3000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    [ReadDate] datetime2 NULL,
    CONSTRAINT [PK_AdvertisementEnquiryMessages] PRIMARY KEY ([AdvertisementEnquiryMessageID]),
    CONSTRAINT [FK_AdvertisementEnquiryMessages_AdvertisementEnquiries_AdvertisementEnquiryID] FOREIGN KEY ([AdvertisementEnquiryID]) REFERENCES [AdvertisementEnquiries] ([AdvertisementEnquiryID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AdvertisementEnquiryMessages_Users_SenderUserID] FOREIGN KEY ([SenderUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AdvertisementEnquiries_RecipientUserID] ON [AdvertisementEnquiries] ([RecipientUserID]);

CREATE INDEX [IX_AdvertisementEnquiryMessages_AdvertisementEnquiryID_CreatedDate] ON [AdvertisementEnquiryMessages] ([AdvertisementEnquiryID], [CreatedDate]);

CREATE INDEX [IX_AdvertisementEnquiryMessages_SenderUserID_IsRead] ON [AdvertisementEnquiryMessages] ([SenderUserID], [IsRead]);

ALTER TABLE [AdvertisementEnquiries] ADD CONSTRAINT [FK_AdvertisementEnquiries_Users_RecipientUserID] FOREIGN KEY ([RecipientUserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923060901_AddAdvertisementEnquiries', N'9.0.9');

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Messages]') AND [c].[name] = N'AdvertisementID');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Messages] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Messages] ALTER COLUMN [AdvertisementID] bigint NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923073253_UpdateMessageAdvertisementNullable', N'9.0.9');

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'Message');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Notifications] ALTER COLUMN [Message] nvarchar(4000) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'IsRead');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Notifications] ADD DEFAULT CAST(0 AS bit) FOR [IsRead];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923074042_UpdateNotificationModel', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260924155011_AlignAdvertisementMediaWithDatabase', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260924160542_AlignCityRelationshipWithDatabase', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260924161618_AlignAreaRelationshipWithDatabase', N'9.0.9');

ALTER TABLE [Advertisements] ADD [ContactEmail] nvarchar(254) NULL;

ALTER TABLE [Advertisements] ADD [ShowEmailToPublic] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Advertisements] ADD [ShowWhatsAppToPublic] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Advertisements] ADD [WhatsAppNumber] nvarchar(30) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260927091706_AddAdvertisementContactInformation', N'9.0.9');

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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260928090314__CanonicalSchemaAudit', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930063954_AlignApprovalRequestColumns', N'9.0.9');


                DELETE FROM [dbo].[RejectionReasons];
            


                ALTER TABLE [dbo].[RejectionReasons]
                DROP CONSTRAINT [UX_RejectionReasons_Code];
            


                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonCode] nvarchar(50) NOT NULL;
            


                ALTER TABLE [dbo].[RejectionReasons]
                ADD CONSTRAINT [UX_RejectionReasons_Code]
                UNIQUE NONCLUSTERED ([ReasonCode]);
            


                ALTER TABLE [dbo].[RejectionReasons]
                ALTER COLUMN [ReasonText] nvarchar(max) NOT NULL;
            


                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [ReasonName];

                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [ReasonNameAr];

                ALTER TABLE [dbo].[RejectionReasons]
                DROP COLUMN [Description];
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930071244_AlignRejectionReasonColumns', N'9.0.9');

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RejectionReasonID', N'ReasonCode', N'ReasonText', N'ReasonTextAr', N'IsActive', N'DisplayOrder') AND [object_id] = OBJECT_ID(N'[RejectionReasons]'))
    SET IDENTITY_INSERT [RejectionReasons] ON;
INSERT INTO [RejectionReasons] ([RejectionReasonID], [ReasonCode], [ReasonText], [ReasonTextAr], [IsActive], [DisplayOrder])
VALUES (1, N'INCOMPLETE_INFORMATION', N'Incomplete advertisement information', N'معلومات الإعلان غير مكتملة', CAST(1 AS bit), 1),
(2, N'INCORRECT_INFORMATION', N'Incorrect or misleading information', N'معلومات غير صحيحة أو مضللة', CAST(1 AS bit), 2),
(3, N'DUPLICATE_ADVERTISEMENT', N'Duplicate advertisement', N'إعلان مكرر', CAST(1 AS bit), 3),
(4, N'INAPPROPRIATE_CONTENT', N'Inappropriate content', N'محتوى غير مناسب', CAST(1 AS bit), 4),
(5, N'PROHIBITED_CONTENT', N'Prohibited content', N'محتوى محظور', CAST(1 AS bit), 5),
(6, N'INVALID_CONTACT', N'Invalid or unreachable contact information', N'معلومات اتصال غير صحيحة أو غير متاحة', CAST(1 AS bit), 6),
(7, N'INVALID_PRICE', N'Invalid or misleading price information', N'معلومات السعر غير صحيحة أو مضللة', CAST(1 AS bit), 7),
(8, N'LOCATION_MISMATCH', N'Advertisement location does not match the submitted information', N'موقع الإعلان لا يتطابق مع المعلومات المقدمة', CAST(1 AS bit), 8),
(9, N'INSUFFICIENT_DOCUMENTATION', N'Required supporting information or documentation is missing', N'المعلومات أو المستندات المطلوبة غير متوفرة', CAST(1 AS bit), 9),
(10, N'OTHER', N'Other reason', N'سبب آخر', CAST(1 AS bit), 10);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RejectionReasonID', N'ReasonCode', N'ReasonText', N'ReasonTextAr', N'IsActive', N'DisplayOrder') AND [object_id] = OBJECT_ID(N'[RejectionReasons]'))
    SET IDENTITY_INSERT [RejectionReasons] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930072322_SeedRejectionReasons', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930074856_AlignAdvertisementMediaModel', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930140409_AddAdvertisementFeatureValueArabic', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930142302_AlignApprovalRequestLegacyColumns', N'9.0.9');

IF OBJECT_ID(N'dbo.SystemSettings', N'U') IS NOT NULL
BEGIN
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
     * Existing databases may not have DataType.
     * Preserve the existing values and initialize DataType as NULL.
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
END;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930171053_AlignSystemSettingsSchema', N'9.0.9');

CREATE TABLE [AdvertisementRemovalRequests] (
    [RemovalRequestID] bigint NOT NULL IDENTITY,
    [AdvertisementID] bigint NOT NULL,
    [UserID] bigint NOT NULL,
    [RequestType] varchar(20) NOT NULL,
    [Reason] nvarchar(500) NULL,
    [RequestStatus] varchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ReviewedBy] bigint NULL,
    [ReviewedDate] datetime2 NULL,
    [ReviewComments] nvarchar(500) NULL,
    CONSTRAINT [PK_AdvertisementRemovalRequests] PRIMARY KEY ([RemovalRequestID])
);

CREATE INDEX [IX_AdvRemovalReq_Ad_Created] ON [AdvertisementRemovalRequests] ([AdvertisementID], [CreatedDate]);

CREATE UNIQUE INDEX [UX_AdvRemovalReq_OnePending] ON [AdvertisementRemovalRequests] ([AdvertisementID]) WHERE [RequestStatus] = 'PENDING';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261008093024_AddAdvertisementRemovalRequests', N'9.0.9');

CREATE TABLE [NotificationPreferences] (
    [NotificationPreferenceID] bigint NOT NULL IDENTITY,
    [UserID] bigint NOT NULL,
    [EmailNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [AdvertisementUpdatesEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FavoriteAdvertisementUpdatesEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([NotificationPreferenceID]),
    CONSTRAINT [FK_NotificationPreferences_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_NotificationPreferences_UserID] ON [NotificationPreferences] ([UserID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261008200226_AddNotificationPreferences', N'9.0.9');

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

