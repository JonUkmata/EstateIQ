# EstateIQ - Real Estate Management System

## Project Information

**Project Name:** EstateIQ  
**Version:** 1.0.0  
**Status:** Sprint 4 completed, ready for merge to main  
**Start Date:** 2026-04-14  
**Last Updated:** 2026-05-11  

## Project Description

EstateIQ is a full-stack real estate management system for managing property listings, companies, agents, users, roles, permissions, property images, property types, property statuses, and map-ready property location data.

The project now includes the Sprint 4 security and image workflow: public registration, email verification, login/logout, JWT access tokens, refresh tokens, role/permission based authorization, protected property management APIs, user management endpoints, and property image upload/gallery support. The Sprint 3 property discovery workflow remains available with paginated and filtered property listings, create/edit/delete flows, property details, and a map page that displays properties with persisted coordinates. The backend follows a layered architecture with controllers, services, repositories, DTOs, AutoMapper profiles, EF Core models, seeders, and automated tests. The frontend is a React/Vite application that consumes the backend API through a local Vite `/api` proxy.

## Technology Stack

### Backend

- **Runtime/Framework:** ASP.NET Core Web API targeting `net9.0`
- **Language:** C# with nullable reference types enabled
- **ORM:** Entity Framework Core 9.0.0
- **Database:** SQL Server / SQL Server LocalDB
- **Caching:** Redis through `StackExchange.Redis` 2.12.14
- **Mapping:** AutoMapper via `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
- **API Documentation:** Swagger/OpenAPI through Swashbuckle 6.6.2 and `Microsoft.AspNetCore.OpenApi` 9.0.10
- **Authentication:** JWT bearer authentication through `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Architecture:** Controller + Service + Repository pattern
- **Validation:** Data annotations on DTOs/models plus service-level business validation
- **Logging:** ASP.NET Core console logging

### Backend Packages

```xml
AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
Microsoft.AspNetCore.OpenApi 9.0.10
Microsoft.AspNetCore.Authentication.JwtBearer 9.0.10
Microsoft.EntityFrameworkCore.Design 9.0.0
Microsoft.EntityFrameworkCore.SqlServer 9.0.0
StackExchange.Redis 2.12.14
Swashbuckle.AspNetCore 6.6.2
```

### Testing

- **Test Framework:** xUnit 2.9.2
- **Integration Testing:** `Microsoft.AspNetCore.Mvc.Testing` 9.0.10
- **Test Database:** `Microsoft.EntityFrameworkCore.InMemory` 9.0.0
- **Current Test Result:** 166 passing tests

### Frontend

- **Framework:** React 19.2.5
- **Build Tool:** Vite 8.x
- **Language:** TypeScript 6.0.2
- **Routing:** React Router DOM 7.14.1
- **Map UI:** Leaflet 1.9.4 and React Leaflet 5.0.0
- **Package Manager:** npm with committed `package-lock.json`

### Not Currently Installed

The repository does not currently include Serilog or FluentValidation. Logging uses built-in ASP.NET Core console logging, and validation uses data annotations plus custom service validation.

## Solution Structure

```text
EstateIQ/
|-- backend/
|   |-- EstateIQ/
|   |   |-- Controllers/        # API controllers
|   |   |-- Constants/          # Roles, permissions, auth policies, upload limits
|   |   |-- Data/               # DbContext, design-time factory, seeders, env loader
|   |   |-- DTOs/               # Request/response contracts
|   |   |-- Exceptions/         # Custom domain/application exceptions
|   |   |-- Extensions/         # ClaimsPrincipal helpers
|   |   |-- Interfaces/         # Service and repository contracts
|   |   |-- Mappings/           # AutoMapper profile
|   |   |-- Migrations/         # EF Core migrations
|   |   |-- Models/             # Entity models
|   |   |-- Repositories/       # EF Core data access
|   |   |-- Services/           # Business logic
|   |   |-- Program.cs          # API startup, DI, middleware, seed execution
|   |   `-- EstateIQ.csproj
|   `-- EstateIQ.Tests/
|       |-- *Tests.cs           # Repository, service, controller, and seeder tests
|       `-- EstateIQ.Tests.csproj
|-- frontend/
|   |-- src/
|   |   |-- components/
|   |   |-- layouts/
|   |   |-- pages/
|   |   |-- routes/
|   |   |-- services/
|   |   `-- styles.css
|   |-- package.json
|   `-- vite.config.ts
|-- docs/
|   `-- sprints/                # Sprint summaries and planning handoff notes
|-- PROJECT-OVERVIEW.md
|-- DEVELOPMENT-LOG.md
|-- README.md
`-- EstateIQ.sln
```

## Database Schema Overview

The current EF Core model uses these primary tables:

- `Companies`
- `Agents`
- `AgentCompanies`
- `Users`
- `Roles`
- `Permissions`
- `UserRoles`
- `RolePermissions`
- `RefreshTokens`
- `EmailVerificationTokens`
- `PasswordResetTokens`
- `Files`
- `CompanyUsers`
- `PropertyTypes`
- `PropertyStatuses`
- `Properties`

### Key Relationships

- `AgentCompanies.AgentId -> Agents.Id`
- `AgentCompanies.CompanyId -> Companies.Id`
- `UserRoles.UserId -> Users.Id`
- `UserRoles.RoleId -> Roles.Id`
- `RolePermissions.RoleId -> Roles.Id`
- `RolePermissions.PermissionId -> Permissions.Id`
- `RefreshTokens.UserId -> Users.Id`
- `EmailVerificationTokens.UserId -> Users.Id`
- `PasswordResetTokens.UserId -> Users.Id`
- `Files.UploadedBy -> Users.Id`
- `CompanyUsers.UserId -> Users.Id`
- `CompanyUsers.CompanyId -> Companies.Id`
- `Agents.UserId -> Users.Id`
- `Properties.PropertyTypeId -> PropertyTypes.Id`
- `Properties.PropertyStatusId -> PropertyStatuses.Id`
- `Properties.CompanyId -> Companies.Id`
- `Properties.AgentId -> Agents.Id`

### Important Constraints And Indexes

- `Agents.Email` is unique.
- `Users.Email` is unique.
- `Roles.Name` and `Permissions.Name` are unique.
- `UserRoles` has a unique composite index on `UserId` and `RoleId`.
- `RolePermissions` has a unique composite index on `RoleId` and `PermissionId`.
- `AgentCompanies` has a unique composite index on `AgentId` and `CompanyId`.
- `Files` has an index on `Entity` and `EntityId`.
- `PropertyTypes.Name` and `PropertyStatuses.Name` are unique.
- `Properties.Price > 0`
- `Properties.Area > 0`
- `Properties.YearBuilt` must be null or between 1800 and the current year.
- `Properties.Latitude` must be null or between -90 and 90.
- `Properties.Longitude` must be null or between -180 and 180.
- `Properties` has indexes for city, price, property type, status, company, and agent.

## Development Connection String Example

Create `backend/EstateIQ/.env` for local development:

```env
ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=EstateIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
Redis__ConnectionString="localhost:6379,abortConnect=false"
Jwt__Key="EstateIQ-Development-Jwt-Key-Replace-In-Production-2026"
```

Frontend development can use `frontend/.env`:

```env
VITE_API_PROXY_TARGET="http://127.0.0.1:5222"
VITE_API_BASE_URL=""
```

## Key Features

### Current Features

- Backend health test endpoint: `GET /api/test`
- Database health endpoint: `GET /api/test/db`
- Redis health endpoint: `GET /api/test/redis`
- Public registration endpoint: `POST /api/auth/register`
- Email verification endpoint: `POST /api/auth/verify-email`
- Login endpoint: `POST /api/auth/login`
- Refresh token endpoint: `POST /api/auth/refresh`
- Logout endpoint: `POST /api/auth/logout`
- User list endpoint: `GET /api/users`
- Company admin creation endpoint: `POST /api/users/company-admins`
- Agent user creation endpoint: `POST /api/users/agents`
- User activation/deactivation endpoint: `PATCH /api/users/{id}/status`
- Property types dropdown endpoint: `GET /api/propertytypes`
- Property statuses dropdown endpoint: `GET /api/propertystatuses`
- Companies dropdown endpoint: `GET /api/companies`
- Agents dropdown endpoint: `GET /api/agents`
- Agents by company filter: `GET /api/agents?companyId={id}`
- Paginated and filtered properties endpoint: `GET /api/properties`
- Property details endpoint: `GET /api/properties/{id}`
- Property creation endpoint: `POST /api/properties`
- Property update endpoint: `PUT /api/properties/{id}`
- Property delete endpoint: `DELETE /api/properties/{id}`
- Property image upload endpoint: `POST /api/properties/{id}/images`
- Property image list endpoint: `GET /api/properties/{id}/images`
- Property image delete endpoint: `DELETE /api/properties/{id}/images/{imageId}`
- JWT authentication with role and permission claims
- Permission-based protection for property write APIs, company/agent management APIs, user management APIs, and image upload/delete APIs
- Properties frontend page with search, filters, pagination, create form, edit/details links, and delete confirmation
- Property details page at `/properties/:id`
- Property edit page at `/properties/:id/edit`
- Property map page at `/map` using Leaflet markers from persisted latitude/longitude values
- Register page at `/register`
- Login page at `/login`
- Verify email page at `/verify-email`
- Protected route wrapper and role-aware navigation
- Property image gallery and upload UI on property details
- Latitude/longitude persistence and API return values for map visualization
- Automatic seed data for property types, statuses, companies, agents, agent-company relationships, and demo properties
- Template `/weatherforecast` endpoint removed during Sprint 3 stabilization

### Planned Features

- Agent/company management pages
- Dashboard metrics backed by real API data
- Real SMTP email delivery
- Forgot password and reset password flow
- Image ordering and cover image support
- ML price prediction API integration
- Geocoding or map-based coordinate picker
- Production deployment configuration
- Stronger frontend test coverage

## Getting Started

### Prerequisites

- Git
- .NET SDK 9.x
- Node.js 22 LTS recommended
- npm
- Docker Desktop for Redis
- SQL Server LocalDB or another SQL Server instance

### Backend Setup

```powershell
dotnet restore .\backend\EstateIQ\EstateIQ.csproj
dotnet tool restore
dotnet ef database update --project .\backend\EstateIQ\EstateIQ.csproj
dotnet run --project .\backend\EstateIQ\EstateIQ.csproj
```

Expected backend URLs:

- `http://localhost:5222`
- `https://localhost:7174`
- `http://localhost:5222/swagger`

### Redis Setup

```powershell
docker pull redis:7-alpine
docker run -d --name estateiq-redis -p 6379:6379 redis:7-alpine
```

If the container already exists:

```powershell
docker start estateiq-redis
```

### Frontend Setup

```powershell
cd .\frontend
npm install
npm run dev
```

Expected frontend URL:

- `http://localhost:5173`

## Access URLs

| Area | URL |
| --- | --- |
| Frontend | `http://localhost:5173` |
| Properties Page | `http://localhost:5173/properties` |
| Property Details | `http://localhost:5173/properties/{id}` |
| Edit Property | `http://localhost:5173/properties/{id}/edit` |
| Property Map | `http://localhost:5173/map` |
| Login | `http://localhost:5173/login` |
| Register | `http://localhost:5173/register` |
| Verify Email | `http://localhost:5173/verify-email` |
| Backend API | `http://localhost:5222` |
| Swagger UI | `http://localhost:5222/swagger` |
| API Health | `http://localhost:5222/api/test` |
| Database Health | `http://localhost:5222/api/test/db` |
| Redis Health | `http://localhost:5222/api/test/redis` |

## Team Members

- Jon Ukmata
- Project contributors listed in Git history and pull requests

## Contact Info

Use the repository issues, pull requests, or the team communication channel for project questions. Keep implementation decisions and ticket completion notes in `DEVELOPMENT-LOG.md` when that file is active in the working tree.

## Sprint Notes

Sprint summaries live under `docs/sprints/`:

- `docs/sprints/sprint-1.md`
- `docs/sprints/sprint-2.md`
- `docs/sprints/sprint-3.md`
- `docs/sprints/sprint-4.md`

## Latest Verification

Last verified on 2026-05-11:

```text
dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj
Result: 166/166 passing

dotnet build backend\EstateIQ\EstateIQ.csproj --configuration Release
Result: passed, 0 warnings, 0 errors

npm run build
Result: passed
```
