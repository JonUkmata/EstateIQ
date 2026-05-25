# EstateIQ

EstateIQ is a full-stack real estate management system with:

- Backend: ASP.NET Core Web API, EF Core, SQL Server, Redis
- Frontend: React 19, Vite 8, TypeScript

This README is the team setup guide for a fresh machine so the project runs locally with the fewest surprises.

## Project Documentation

Start here when onboarding a new developer or AI assistant:

- [PROJECT-OVERVIEW.md](PROJECT-OVERVIEW.md) - static project overview, stack, architecture, database schema, setup, URLs, and feature status.
- [docs/README.md](docs/README.md) - docs index, supporting documents, and sprint planning notes.
- [docs/ml-price-prediction-flow.md](docs/ml-price-prediction-flow.md) - current ML price generation flow, backend mapping, defaults, validation, and local testing.
- [docs/sprints/sprint-5.md](docs/sprints/sprint-5.md) - Sprint 5 delivered tickets, key files, and verification results.

## Required Tools

Install these before cloning:

- Git
- .NET SDK 9.x
- Node.js 22 LTS recommended
- Docker Desktop
- SQL Server LocalDB recommended on Windows, or another SQL Server instance

Version notes:

- The backend project targets `net9.0`, so teammates need the .NET 9 SDK, not only the runtime.
- The frontend uses Vite 8, which requires Node.js `20.19+` or `22.12+`. Recommended: Node 22 LTS.
- This repo uses `npm` because `package-lock.json` is committed.

Useful checks:

```powershell
dotnet --list-sdks
node -v
npm -v
docker --version
```

## Clone The Repository

```powershell
git clone https://github.com/JonUkmata/EstateIQ.git
cd EstateIQ
```

## Backend Setup

### 1. Create The Backend `.env`

From the repo root:

```powershell
Copy-Item .\backend\EstateIQ\.env.example .\backend\EstateIQ\.env
```

Open `backend/EstateIQ/.env` and fill it like this for the recommended Windows setup:

```env
ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=EstateIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
Redis__ConnectionString="localhost:6379,abortConnect=false"
Jwt__Key="EstateIQ-Development-Jwt-Key-Replace-In-Production-2026"
```

If LocalDB is not installed and the teammate uses another SQL Server instance, only change the first line.

Examples:

```env
ConnectionStrings__DefaultConnection="Server=localhost;Database=EstateIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

```env
ConnectionStrings__DefaultConnection="Server=YOUR_SERVER_NAME;Database=EstateIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

### 2. Restore .NET Dependencies And EF Tooling

From the repo root:

```powershell
dotnet restore .\backend\EstateIQ\EstateIQ.csproj
dotnet tool restore
```

If `dotnet ef` says it is missing, `dotnet tool restore` is the fix.

Important auth note:

- `appsettings.Development.json` includes a development JWT key.
- `appsettings.json` intentionally does not include a production signing key.
- Outside local development, set `Jwt__Key` from environment/configuration.

### 3. Apply Database Migrations

From the repo root:

```powershell
dotnet ef database update --project .\backend\EstateIQ\EstateIQ.csproj
```

### 4. Run The Backend

From the repo root:

```powershell
dotnet run --project .\backend\EstateIQ\EstateIQ.csproj
```

Expected local backend URLs:

- `http://localhost:5222`
- `https://localhost:7174`
- Swagger UI: `http://localhost:5222/swagger`

## Redis Setup With Docker

Start Docker Desktop first, then run:

```powershell
docker pull redis:7-alpine
docker run -d --name estateiq-redis -p 6379:6379 redis:7-alpine
```

Useful Redis commands:

```powershell
docker ps
docker logs estateiq-redis
docker exec -it estateiq-redis redis-cli ping
```

Expected result from the ping command:

```text
PONG
```

If the container already exists:

```powershell
docker start estateiq-redis
```

If the old container is broken and must be recreated:

```powershell
docker rm -f estateiq-redis
docker run -d --name estateiq-redis -p 6379:6379 redis:7-alpine
```

Important note:

- The backend is currently configured to continue starting even if Redis is down.
- `GET /api/test` can still succeed without Redis.
- To verify Redis specifically, use `GET /api/test/redis`.

## Frontend Setup

### 1. Create The Frontend `.env`

From the repo root:

```powershell
Copy-Item .\frontend\.env.example .\frontend\.env
```

For normal local development, keep:

```env
VITE_API_PROXY_TARGET="http://127.0.0.1:5222"
VITE_API_BASE_URL=""
```

Why:

- `VITE_API_BASE_URL=""` keeps API calls relative, for example `/api/test`
- Vite proxies `/api` to the backend on `http://127.0.0.1:5222`

### 2. Install Frontend Dependencies

```powershell
cd .\frontend
npm install
```

### 3. Run The Frontend

```powershell
npm run dev
```

Expected frontend URL:

- `http://localhost:5173`

## Application Routes

### Frontend Routes

| Route | Access | Description |
| --- | --- | --- |
| `/` | Public | Redirects authenticated users by role; shows landing for guests |
| `/login` | Public | Login page |
| `/register` | Public | Registration page |
| `/verify-email` | Public | Email verification page |
| `/properties` | Public | Marketplace card grid with search, filters, and pagination |
| `/properties/:id` | Public | Property details page with image gallery |
| `/map` | Public | Full map search with Leaflet markers and sidebar |
| `/dashboard` | Authenticated | Role-based dashboard with metrics and shortcuts |
| `/properties/new` | CreateProperty permission | Create a new property listing |
| `/properties/:id/edit` | EditProperty permission | Edit an existing property |
| `/my-properties` | Agent | Agent's own property listings |
| `/company/agents` | CompanyAdmin | Manage company agents and create new agents |
| `/admin/users` | Admin | Manage all users and create CompanyAdmins |
| `/dev/health` | Public | Backend smoke-test page (developer use) |

### Backend API Routes

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/test` | Public | API health check |
| GET | `/api/test/db` | Public | Database health check |
| GET | `/api/test/redis` | Public | Redis health check |
| POST | `/api/auth/register` | Public | Register a new user account |
| POST | `/api/auth/verify-email` | Public | Verify email with token |
| POST | `/api/auth/login` | Public | Login and receive JWT + refresh token |
| POST | `/api/auth/refresh` | Public | Refresh an access token |
| POST | `/api/auth/logout` | Authenticated | Logout and revoke refresh token |
| GET | `/api/dashboard/me` | Authenticated | Role-based dashboard statistics |
| GET | `/api/users` | Admin | Paginated user list with filters |
| POST | `/api/users/company-admins` | Admin | Create a CompanyAdmin account |
| POST | `/api/users/agents` | Admin, CompanyAdmin | Create an Agent account |
| PATCH | `/api/users/{id}/status` | Admin | Activate or deactivate a user |
| GET | `/api/properties` | Public | Paginated and filtered property list |
| GET | `/api/properties/{id}` | Public | Property details |
| POST | `/api/properties` | CreateProperty | Create a property |
| POST | `/api/properties/generate-price` | CreateProperty | Generate an ML suggested listing price |
| PUT | `/api/properties/{id}` | EditProperty | Update a property |
| DELETE | `/api/properties/{id}` | DeleteProperty | Delete a property |
| GET | `/api/properties/{id}/images` | Public | List property images |
| POST | `/api/properties/{id}/images` | UploadPropertyImages | Upload images to a property |
| DELETE | `/api/properties/{id}/images/{imageId}` | UploadPropertyImages | Delete a property image |
| GET | `/api/agents` | Authenticated | List agents, optionally filtered by company |
| GET | `/api/agents/my-company` | CompanyAdmin | List agents for the authenticated CompanyAdmin's company |
| GET | `/api/companies` | Authenticated | List companies |
| GET | `/api/propertytypes` | Public | List property types |
| GET | `/api/propertystatuses` | Public | List property statuses |

## Demo Auth Flow

This project uses a simulated email verification flow because no real SMTP is configured. Use these steps to create and verify an account locally:

1. Open `http://localhost:5173/register`.
2. Fill in the registration form and submit.
3. A demo verification token is shown on screen. Copy it, or click the prefilled verification link.
4. The verify page (`/verify-email`) accepts the token from the link or from manual paste.
5. After verification, go to `/login` and sign in.
6. You land on `/dashboard` (Admin, CompanyAdmin, Agent) or `/properties` (User) based on your role.

Seed data creates initial roles and permissions automatically. To get an Admin account, use the seeded admin user credentials configured in `Program.cs` or create one directly in the database.

## Role-Based Navigation

After login, the navigation adapts to the authenticated user's role:

| Role | Navigation Items |
| --- | --- |
| Guest (logged out) | Login, Register |
| User | Properties, Map Search, Dashboard, Logout |
| Agent | Dashboard, Properties, Map Search, My Properties, Logout |
| CompanyAdmin | Dashboard, Properties, Map Search, Company Agents, Logout |
| Admin | Dashboard, Properties, Map Search, Admin Users, Logout |

Role precedence for dashboard dispatch is: **Admin > CompanyAdmin > Agent > User**.

All role-specific routes are enforced by both frontend `ProtectedRoute` wrappers and backend authorization policies. Hiding a link in the UI does not grant access; the backend rejects unauthorized requests independently.

## User Marketplace Flow

Any visitor, authenticated or not, can browse properties and the map without logging in.

- `/properties` shows a card grid with search, city filter, type, status, and price range filters.
- `/properties/:id` shows full property details including images.
- `/map` shows all properties with valid coordinates as Leaflet markers. The sidebar lists the same properties. Clicking a sidebar card flies the map to the marker. Clicking a popup opens the details page.
- "View on Map" and "View as List" links carry current filters between the two views.

## Admin and CompanyAdmin Management Flow

### Admin creates a CompanyAdmin

1. Log in as Admin.
2. Go to `/admin/users`.
3. Scroll to the "Add CompanyAdmin" form.
4. Fill in first name, last name, email, temporary password, and select a company.
5. Submit. The new CompanyAdmin appears in the user list immediately.

### CompanyAdmin creates an Agent

1. Log in as CompanyAdmin.
2. Go to `/company/agents`.
3. Fill in the "Add Agent" form: first name, last name, email, temporary password, and optional phone.
4. The backend derives the company from the authenticated CompanyAdmin's account. No company field is shown.
5. Submit. The new Agent appears in the agent list immediately.

### Activate and deactivate users

Admin can activate or deactivate any user from the `/admin/users` page. Deactivation shows a confirmation dialog and immediately revokes active sessions for that user. Admins cannot deactivate their own account from this UI.

## Dashboard and Redis Caching

`GET /api/dashboard/me` returns role-specific statistics. The response is cached in Redis per role and scope to avoid repeated SQL queries.

### Cache keys

| Role | Cache Key |
| --- | --- |
| Admin | `dashboard:admin:global` |
| CompanyAdmin | `dashboard:companyadmin:company:{companyId}` |
| Agent | `dashboard:agent:{agentId}` |
| User | `dashboard:user:marketplace` |

Cache TTL is 30 minutes as a safety fallback. The primary freshness mechanism is invalidation, not expiry.

If Redis is unavailable, the dashboard falls back to a direct SQL query. The client receives the same response and is not told that Redis was skipped.

### Cache invalidation

Caches are invalidated immediately at the service layer when data changes:

| Event | Keys invalidated |
| --- | --- |
| Property created, updated, or deleted | `admin:global`, `companyadmin:company:{id}`, `agent:{id}`, `user:marketplace` |
| Agent created or status changed | `admin:global`, `companyadmin:company:{id}` |
| CompanyAdmin created or status changed | `admin:global`, `companyadmin:company:{id}` |
| Regular user status changed | `admin:global` |

When a property is updated and its company or agent changes, both the old and new scopes are invalidated.

## Smoke Test Checklist

Run these in order during the call:

1. Confirm Redis container is running.
2. Confirm backend starts without configuration errors.
3. Open `http://localhost:5222/api/test` and expect `API is running`.
4. Open `http://localhost:5222/api/test/db` and expect database success.
5. Open `http://localhost:5222/api/test/redis` and expect Redis success.
6. Start the frontend and open `http://localhost:5173`.
7. Confirm the landing page shows Login and Register options.
8. Open `http://localhost:5173/register`, create a user, and copy the generated verification token.
9. Open `http://localhost:5173/verify-email?token=PASTE_TOKEN_HERE` and verify the user.
10. Login at `http://localhost:5173/login`. Confirm redirect to `/properties` for a User role.
11. Confirm public property browsing at `/properties` shows the card grid.
12. Confirm `/map` shows the Leaflet map with markers and sidebar.
13. Confirm `/dashboard` shows role-appropriate metrics.
14. If testing Admin: confirm `/admin/users` loads the user list.
15. If testing CompanyAdmin: confirm `/company/agents` loads the agent list.

## Common Problems

### `dotnet ef` Is Not Available

Run:

```powershell
dotnet tool restore
```

### Frontend Works But Redis Is Actually Off

This is expected if the frontend only calls `GET /api/test`.

Use:

```text
http://localhost:5222/api/test/redis
```

to verify Redis itself.

### Port `5222` Is Already In Use

Another backend process is probably still running. Stop the old `EstateIQ.exe` or old `dotnet` process, then start again.

### Docker Says The Redis Name Already Exists

Run:

```powershell
docker start estateiq-redis
```

or recreate it:

```powershell
docker rm -f estateiq-redis
docker run -d --name estateiq-redis -p 6379:6379 redis:7-alpine
```

### SQL Server Connection Fails

Check:

- SQL Server LocalDB or SQL Server is installed
- The connection string in `backend/EstateIQ/.env` points to the correct server
- The teammate ran `dotnet ef database update`

### Frontend Cannot Reach Backend

Check:

- Backend is running on `http://localhost:5222`
- `frontend/.env` still has `VITE_API_PROXY_TARGET="http://127.0.0.1:5222"`
- The frontend was restarted after changing `.env`

### Backend Fails With `JWT key is not configured`

Set a local JWT key in `backend/EstateIQ/.env`:

```env
Jwt__Key="EstateIQ-Development-Jwt-Key-Replace-In-Production-2026"
```

The key must be at least 32 bytes long.

### Uploaded Images Should Not Be Committed

Runtime uploads are written under:

```text
backend/EstateIQ/wwwroot/uploads/
```

That folder is ignored by Git and should stay local/runtime-only.

## Recommended Order During Team Onboarding

1. Install prerequisites.
2. Clone the repo.
3. Create both `.env` files from the examples.
4. Start Redis in Docker.
5. Run `dotnet restore`.
6. Run `dotnet tool restore`.
7. Run `dotnet ef database update`.
8. Run the backend.
9. Run `npm install` in `frontend`.
10. Run the frontend.
11. Check `/api/test`, `/api/test/db`, `/api/test/redis`, and the home page.
12. Smoke test register, verify email, login, `/properties`, and `/map`.

## Sprint Notes

Sprint summaries live under `docs/sprints/`:

- [docs/sprints/sprint-1.md](docs/sprints/sprint-1.md)
- [docs/sprints/sprint-2.md](docs/sprints/sprint-2.md)
- [docs/sprints/sprint-3.md](docs/sprints/sprint-3.md)
- [docs/sprints/sprint-4.md](docs/sprints/sprint-4.md)
- [docs/sprints/sprint-5.md](docs/sprints/sprint-5.md)

## References

- Vite getting started and Node version requirements: https://vite.dev/guide/
