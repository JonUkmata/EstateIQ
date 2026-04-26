# EstateIQ - Real Estate Management System

## Project Information

**Project Name:** EstateIQ  
**Version:** 1.0.0  
**Status:** In Development  
**Start Date:** 2026-04-14  
**Last Updated:** 2026-04-26  

## Project Description

EstateIQ is a full-stack real estate management system for managing property listings, companies, agents, property types, property statuses, and location-ready property data. The current sprint work focuses on property management APIs and frontend workflows. The next planned map work depends on persisted `Latitude` and `Longitude` values returned by the Properties API.

The project uses a layered backend architecture with controllers, services, repositories, DTOs, AutoMapper profiles, Entity Framework Core models, and integration/unit tests. The frontend is a React/Vite application that consumes the backend API through a Vite `/api` proxy during local development.

## Technology Stack

### Backend

- **Runtime/Framework:** ASP.NET Core Web API targeting `net9.0`
- **Language:** C# with nullable reference types enabled
- **ORM:** Entity Framework Core 9.0.0
- **Database:** SQL Server / SQL Server LocalDB
- **Caching:** Redis through `StackExchange.Redis` 2.12.14
- **Mapping:** AutoMapper via `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
- **API Documentation:** Swagger/OpenAPI through Swashbuckle 6.6.2 and `Microsoft.AspNetCore.OpenApi` 9.0.10
- **Architecture:** Controller + Service + Repository pattern
- **Validation:** Data annotations on DTOs/models plus service-level business validation
- **Logging:** ASP.NET Core console logging

### Backend Packages

```xml
AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
Microsoft.AspNetCore.OpenApi 9.0.10
Microsoft.EntityFrameworkCore.Design 9.0.0
Microsoft.EntityFrameworkCore.SqlServer 9.0.0
StackExchange.Redis 2.12.14
Swashbuckle.AspNetCore 6.6.2
```

### Testing

- **Test Framework:** xUnit 2.9.2
- **Integration Testing:** `Microsoft.AspNetCore.Mvc.Testing` 9.0.10
- **Test Database:** `Microsoft.EntityFrameworkCore.InMemory` 9.0.0
- **Current Test Result:** 49 passing tests

### Frontend

- **Framework:** React 19.2.5
- **Build Tool:** Vite 8.0.4
- **Language:** TypeScript 6.0.2
- **Routing:** React Router DOM 7.14.1
- **Package Manager:** npm with committed `package-lock.json`

### Not Currently Installed

The request example mentions Serilog and FluentValidation, but the current repository does not include those packages. Logging currently uses built-in ASP.NET Core console logging, and validation currently uses data annotations plus custom service validation.

## Solution Structure

```text
EstateIQ/
├── backend/
│   ├── EstateIQ/
│   │   ├── Controllers/        # API controllers
│   │   ├── Data/               # DbContext, design-time factory, seeders, env loader
│   │   ├── DTOs/               # Request/response contracts
│   │   ├── Exceptions/         # Custom domain/application exceptions
│   │   ├── Interfaces/         # Service and repository contracts
│   │   ├── Mappings/           # AutoMapper profile
│   │   ├── Migrations/         # EF Core migrations
│   │   ├── Models/             # Entity models
│   │   ├── Repositories/       # EF Core data access
│   │   ├── Services/           # Business logic
│   │   ├── Program.cs          # API startup, DI, middleware, seed execution
│   │   └── EstateIQ.csproj
│   └── EstateIQ.Tests/
│       ├── *Tests.cs           # Repository, service, controller, and seeder tests
│       └── EstateIQ.Tests.csproj
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── layouts/
│   │   ├── pages/
│   │   ├── routes/
│   │   ├── services/
│   │   └── styles.css
│   ├── package.json
│   └── vite.config.ts
├── docs/                       # Supporting SQL/schema/workflow docs
├── PROJECT-OVERVIEW.md
├── DEVELOPMENT-LOG.md
├── README.md
└── EstateIQ.sln
```

## Database Schema Overview

The current EF Core model uses these primary tables:

- `Companies`
- `Agents`
- `AgentCompanies`
- `PropertyTypes`
- `PropertyStatuses`
- `Properties`

### Key Relationships

- `AgentCompanies.AgentId -> Agents.Id`
- `AgentCompanies.CompanyId -> Companies.Id`
- `Properties.PropertyTypeId -> PropertyTypes.Id`
- `Properties.PropertyStatusId -> PropertyStatuses.Id`
- `Properties.CompanyId -> Companies.Id`
- `Properties.AgentId -> Agents.Id`

### Important Constraints And Indexes

- `Agents.Email` is unique.
- `AgentCompanies` has a unique composite index on `AgentId` and `CompanyId`.
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
- Property types dropdown endpoint: `GET /api/propertytypes`
- Property statuses dropdown endpoint: `GET /api/propertystatuses`
- Companies dropdown endpoint: `GET /api/companies`
- Agents dropdown endpoint: `GET /api/agents`
- Agents by company filter: `GET /api/agents?companyId={id}`
- Properties list endpoint: `GET /api/properties`
- Property details endpoint: `GET /api/properties/{id}`
- Property creation endpoint: `POST /api/properties`
- Properties frontend page with list, create form, dropdowns, validations, and API refresh after submit
- Latitude/longitude persistence and API return values for future map features
- Automatic seed data for property types, statuses, companies, agents, agent-company relationships, and demo properties

### Planned Features

- Sprint 3 map visualization using persisted property coordinates
- Property editing workflow
- Property search and filtering UI
- Authentication and authorization
- Agent/company management pages
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
| Backend API | `http://localhost:5222` |
| Swagger UI | `http://localhost:5222/swagger` |
| API Health | `http://localhost:5222/api/test` |
| Database Health | `http://localhost:5222/api/test/db` |
| Redis Health | `http://localhost:5222/api/test/redis` |

## Team Members

- Jon Ukmata
- Project contributors listed in Git history and pull requests

## Contact Info

Use the repository issues, pull requests, or the team communication channel for project questions. Keep implementation decisions and ticket completion notes in `DEVELOPMENT-LOG.md`.

