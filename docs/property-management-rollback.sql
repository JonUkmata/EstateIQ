BEGIN TRANSACTION;
DROP TABLE [AgentCompanies];

DROP TABLE [Properties];

DROP TABLE [Agents];

DROP TABLE [Companies];

DROP TABLE [PropertyStatuses];

DROP TABLE [PropertyTypes];

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260422084422_InitialCreate';

COMMIT;
GO

