# EstateIQ

EstateIQ is a full-stack real estate management system with:

- Backend: ASP.NET Core Web API, EF Core, SQL Server, Redis
- Frontend: React 19, Vite 8, TypeScript

This README is the team setup guide for a fresh machine so the project runs locally with the fewest surprises.

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

## Smoke Test Checklist

Run these in order during the call:

1. Confirm Redis container is running.
2. Confirm backend starts without configuration errors.
3. Open `http://localhost:5222/api/test` and expect `API is running`.
4. Open `http://localhost:5222/api/test/db` and expect database success.
5. Open `http://localhost:5222/api/test/redis` and expect Redis success.
6. Start the frontend and open `http://localhost:5173`.
7. On the home page, confirm the backend response card shows success.

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

## References

- Vite getting started and Node version requirements: https://vite.dev/guide/
