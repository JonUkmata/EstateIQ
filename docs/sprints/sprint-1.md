# Sprint 1 - Project Setup And Foundation

## Sprint Goal

Establish the EstateIQ development foundation so backend, frontend, database, Redis, local setup, and testing can run consistently on developer machines.

## Sprint Status

**Status:** Completed  
**Primary Theme:** Setup, tooling, architecture foundation, onboarding  
**Result:** The project can be cloned, configured, run locally, and tested.

## What Was Delivered

### Backend Foundation

- ASP.NET Core Web API project under `backend/EstateIQ`.
- `Program.cs` configured with controllers, Swagger/OpenAPI, EF Core, Redis, AutoMapper, and dependency injection.
- SQL Server connection string loading through environment/configuration.
- Redis connection setup through `StackExchange.Redis`.
- Health/smoke endpoints:
  - `GET /api/test`
  - `GET /api/test/db`
  - `GET /api/test/redis`

### Frontend Foundation

- React/Vite/TypeScript frontend under `frontend`.
- App routing through React Router.
- Layout structure with navbar/sidebar/content areas.
- API helper foundation in `frontend/src/services/api.ts`.
- Vite proxy support for `/api` requests to the backend.

### Database And Tooling

- EF Core setup with SQL Server provider.
- Design-time DbContext factory for EF tooling.
- Initial EF migration structure.
- LocalDB-oriented setup path for development.

### Testing Foundation

- xUnit test project under `backend/EstateIQ.Tests`.
- ASP.NET Core integration testing package configured.
- EF Core InMemory package configured for tests.

### Documentation And Onboarding

- README local setup guide covering:
  - required tools
  - backend `.env`
  - Redis Docker setup
  - database migrations
  - frontend `.env`
  - smoke test checklist
  - common local problems

## Key Files

- `/EstateIQ.sln`
- `/README.md`
- `/backend/EstateIQ/EstateIQ.csproj`
- `/backend/EstateIQ/Program.cs`
- `/backend/EstateIQ/Data/AppDbContext.cs`
- `/backend/EstateIQ/Data/DesignTimeDbContextFactory.cs`
- `/backend/EstateIQ/Data/EnvironmentFileLoader.cs`
- `/backend/EstateIQ.Tests/EstateIQ.Tests.csproj`
- `/frontend/package.json`
- `/frontend/vite.config.ts`
- `/frontend/src/App.tsx`
- `/frontend/src/routes/AppRouter.tsx`
- `/frontend/src/services/api.ts`

## Architecture Established

- Controller layer for HTTP endpoints.
- Service layer for business logic.
- Repository layer for database access.
- DTO layer for API contracts.
- EF Core model layer for persisted entities.
- Test project separated from production API.

## Acceptance Notes

- Local development depends on `.env` files that are not committed.
- Backend currently targets `net9.0`.
- Frontend uses React 19 and Vite 8.
- Swagger is available in development at `/swagger`.

## What Sprint 1 Enabled

Sprint 1 made it possible to begin feature work without re-solving setup problems. Sprint 2 could then focus on property management, seed data, lookup APIs, frontend property workflows, and tests.

