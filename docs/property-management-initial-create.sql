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
CREATE TABLE [Agents] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Phone] nvarchar(50) NULL,
    [Mobile] nvarchar(50) NULL,
    [PhotoUrl] nvarchar(500) NULL,
    [Bio] nvarchar(1000) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Agents] PRIMARY KEY ([Id])
);

CREATE TABLE [Companies] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(50) NULL,
    [Address] nvarchar(300) NULL,
    [City] nvarchar(100) NULL,
    [Website] nvarchar(200) NULL,
    [LogoUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);

CREATE TABLE [PropertyStatuses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ColorCode] nvarchar(7) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_PropertyStatuses] PRIMARY KEY ([Id])
);

CREATE TABLE [PropertyTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_PropertyTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [AgentCompanies] (
    [Id] int NOT NULL IDENTITY,
    [AgentId] int NOT NULL,
    [CompanyId] int NOT NULL,
    [Role] nvarchar(100) NULL,
    [JoinedDate] date NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_AgentCompanies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AgentCompanies_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AgentCompanies_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Properties] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [Area] decimal(10,2) NOT NULL,
    [Bedrooms] int NULL,
    [Bathrooms] int NULL,
    [Floors] int NULL,
    [YearBuilt] int NULL,
    [PropertyTypeId] int NOT NULL,
    [PropertyStatusId] int NOT NULL,
    [CompanyId] int NOT NULL,
    [AgentId] int NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Latitude] decimal(10,8) NULL,
    [Longitude] decimal(11,8) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Properties] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Properties_Area] CHECK ([Area] > 0),
    CONSTRAINT [CK_Properties_Latitude] CHECK ([Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)),
    CONSTRAINT [CK_Properties_Longitude] CHECK ([Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)),
    CONSTRAINT [CK_Properties_Price] CHECK ([Price] > 0),
    CONSTRAINT [CK_Properties_YearBuilt] CHECK ([YearBuilt] IS NULL OR ([YearBuilt] >= 1800 AND [YearBuilt] <= YEAR(GETDATE()))),
    CONSTRAINT [FK_Properties_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Properties_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Properties_PropertyStatuses_PropertyStatusId] FOREIGN KEY ([PropertyStatusId]) REFERENCES [PropertyStatuses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Properties_PropertyTypes_PropertyTypeId] FOREIGN KEY ([PropertyTypeId]) REFERENCES [PropertyTypes] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ColorCode', N'CreatedAt', N'Description', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[PropertyStatuses]'))
    SET IDENTITY_INSERT [PropertyStatuses] ON;
INSERT INTO [PropertyStatuses] ([Id], [ColorCode], [CreatedAt], [Description], [IsActive], [Name])
VALUES (1, N'#007bff', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'For Sale'),
(2, N'#ffc107', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'For Rent'),
(3, N'#28a745', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Sold'),
(4, N'#17a2b8', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Rented'),
(5, N'#6c757d', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Off Market'),
(6, N'#fd7e14', '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Under Contract');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ColorCode', N'CreatedAt', N'Description', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[PropertyStatuses]'))
    SET IDENTITY_INSERT [PropertyStatuses] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[PropertyTypes]'))
    SET IDENTITY_INSERT [PropertyTypes] ON;
INSERT INTO [PropertyTypes] ([Id], [CreatedAt], [Description], [IsActive], [Name])
VALUES (1, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Apartment'),
(2, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'House'),
(3, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Villa'),
(4, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Office'),
(5, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Land'),
(6, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Commercial'),
(7, '2026-04-22T00:00:00.0000000Z', NULL, CAST(1 AS bit), N'Penthouse');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[PropertyTypes]'))
    SET IDENTITY_INSERT [PropertyTypes] OFF;

CREATE UNIQUE INDEX [IX_AgentCompanies_AgentId_CompanyId] ON [AgentCompanies] ([AgentId], [CompanyId]);

CREATE INDEX [IX_AgentCompanies_CompanyId] ON [AgentCompanies] ([CompanyId]);

CREATE UNIQUE INDEX [IX_Agents_Email] ON [Agents] ([Email]);

CREATE INDEX [IX_Properties_AgentId] ON [Properties] ([AgentId]);

CREATE INDEX [IX_Properties_City] ON [Properties] ([City]);

CREATE INDEX [IX_Properties_CompanyId] ON [Properties] ([CompanyId]);

CREATE INDEX [IX_Properties_Price] ON [Properties] ([Price]);

CREATE INDEX [IX_Properties_PropertyStatusId] ON [Properties] ([PropertyStatusId]);

CREATE INDEX [IX_Properties_PropertyTypeId] ON [Properties] ([PropertyTypeId]);

CREATE UNIQUE INDEX [IX_PropertyStatuses_Name] ON [PropertyStatuses] ([Name]);

CREATE UNIQUE INDEX [IX_PropertyTypes_Name] ON [PropertyTypes] ([Name]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260422084422_InitialCreate', N'9.0.0');

COMMIT;
GO

