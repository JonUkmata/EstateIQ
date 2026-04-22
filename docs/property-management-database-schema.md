# Property Management Database Schema

This schema defines the initial SQL Server structure for the EstateIQ property management module.

## Tables

### `Companies`
- Stores real estate company records.
- One company can manage many properties.
- One company can be linked to many agents through `AgentCompanies`.

### `Agents`
- Stores agent profile and contact data.
- `Email` is unique.
- One agent can manage many properties.

### `AgentCompanies`
- Junction table for the many-to-many relationship between `Agents` and `Companies`.
- Prevents duplicate pairs with a unique index on `(AgentId, CompanyId)`.
- Both foreign keys use cascade delete.

### `PropertyTypes`
- Lookup table for property categories.
- Seeded values:
  `Apartment`, `House`, `Villa`, `Office`, `Land`, `Commercial`, `Penthouse`

### `PropertyStatuses`
- Lookup table for listing status and UI color.
- Seeded values:
  `For Sale`, `For Rent`, `Sold`, `Rented`, `Off Market`, `Under Contract`

### `Properties`
- Main property table.
- Linked to `PropertyTypes`, `PropertyStatuses`, `Companies`, and `Agents`.
- Includes validation constraints for:
  `Price`, `Area`, `YearBuilt`, `Latitude`, `Longitude`

## Relationships

- `AgentCompanies.AgentId -> Agents.Id` with `ON DELETE CASCADE`
- `AgentCompanies.CompanyId -> Companies.Id` with `ON DELETE CASCADE`
- `Properties.PropertyTypeId -> PropertyTypes.Id` with restricted delete
- `Properties.PropertyStatusId -> PropertyStatuses.Id` with restricted delete
- `Properties.CompanyId -> Companies.Id` with restricted delete
- `Properties.AgentId -> Agents.Id` with restricted delete

## Indexes

- `IX_Agents_Email` unique
- `IX_AgentCompanies_AgentId_CompanyId` unique
- `IX_Properties_City`
- `IX_Properties_Price`
- `IX_Properties_PropertyTypeId`
- `IX_Properties_PropertyStatusId`
- `IX_Properties_CompanyId`
- `IX_Properties_AgentId`
- `IX_PropertyTypes_Name` unique
- `IX_PropertyStatuses_Name` unique

## Migration Artifacts

- EF Core migration:
  [backend/EstateIQ/Migrations/20260422084422_InitialCreate.cs](../backend/EstateIQ/Migrations/20260422084422_InitialCreate.cs)
- Apply script:
  [docs/property-management-initial-create.sql](./property-management-initial-create.sql)
- Rollback script:
  [docs/property-management-rollback.sql](./property-management-rollback.sql)

## Usage

Apply the EF migration:

```powershell
dotnet ef database update --project backend/EstateIQ/EstateIQ.csproj --startup-project backend/EstateIQ/EstateIQ.csproj
```

Or execute the generated SQL script in SSMS:

```text
docs/property-management-initial-create.sql
```
