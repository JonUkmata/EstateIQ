# Sprint 5 - Role-Based UI, Redis Dashboard Caching, and Management Flows

## Sprint Goal

Deliver a role-aware frontend experience in EstateIQ. A regular User uses the platform as a property marketplace with card-based browsing and full map search. A CompanyAdmin manages their company's agents from a dedicated UI. An Admin manages users and creates CompanyAdmins. All roles see a dashboard with real statistics pulled from `GET /api/dashboard/me`. Dashboard responses are cached in Redis per role and scope, and caches are invalidated immediately when relevant data changes.

## Sprint Status

**Status:** Completed
**Primary Theme:** Role-based UI, user management flows, Redis dashboard caching, shared UX consistency
**Result:** All 20 Sprint 5 tickets delivered. Frontend build passes. 188/188 backend tests passing.

## Tickets Delivered

### ES-142 — Improve Unauthenticated Landing Flow

- `/` now redirects authenticated users to `/dashboard` (Admin, CompanyAdmin, Agent) or `/properties` (User).
- Unauthenticated users land on an auth landing page with clear Login and Register options.
- Old smoke-test page moved to `/dev/health`.

### ES-143 — Redesign Login Page UI

- Login page redesigned with a modern two-panel layout.
- Clear email/password form with inline field errors.
- Loading state shown during submit.
- Unverified email produces a human-readable message.
- Role-based redirect after successful login.

### ES-144 — Redesign Register and Verify Email UI

- Register page redesigned with clean account details form.
- Demo verification token shown immediately after registration, with a prefilled verify link.
- Verify email page supports both query string token and manual paste.
- UI copy explains that verification is simulated (no real SMTP).

### ES-145 — Implement Role-Based Navigation Cleanup

- Navigation links change based on the authenticated role.
- Logged-out: Login, Register.
- User: Properties, Map Search, Dashboard, Logout.
- Agent: Dashboard, Properties, Map Search, My Properties, Logout.
- CompanyAdmin: Dashboard, Properties, Map Search, Company Agents, Logout.
- Admin: Dashboard, Properties, Map Search, Admin Users, Logout.

### ES-146 — Convert Properties Page to Marketplace Card Grid

- Properties page converted from table to responsive card grid.
- Each card shows cover image or placeholder, price, title or type + city, city, type, status badge, area/bedrooms/bathrooms, and a Details button.
- Edit and Delete actions shown only for authorized roles.
- Backend now returns `coverImageUrl` in the property list response.

### ES-147 — Separate Create Property Flow from Marketplace Page

- Create Property moved to a dedicated route: `/properties/new`.
- Accessible only to users with the `CreateProperty` permission.
- After successful creation, user is redirected to the new property's details page.
- Marketplace page `/properties` is now purely for browsing.

### ES-148 — Improve Full Map Search Page for Users

- `/map` rebuilt as a full map search experience with Leaflet.
- Sidebar shows property cards with image, price, and city.
- Marker popup shows mini property card with a View Details link.
- Clicking a sidebar card flies the map to the marker and opens the popup.
- Filters mirror Properties page: search, city, type, status, min/max price.

### ES-149 — Connect Marketplace List View and Map Search View

- Properties page has a "View on Map" link that carries current query params.
- Map page has a "View as List" link that carries current filters.
- Users retain context when switching between list and map views.

### ES-150 — Build CompanyAdmin Agents Management Page

- `/company/agents` page for CompanyAdmins.
- Shows all agents belonging to the authenticated CompanyAdmin's company.
- Columns: first name, last name, email, status, created date.
- Route protected; regular Users and Agents cannot access it.

### ES-151 — Add Create Agent Form for CompanyAdmin

- Create Agent form added to `/company/agents`.
- Fields: first name, last name, email, temporary password, phone.
- CompanyAdmin does not select a company; the backend derives it from the authenticated user.
- Form validation with inline errors. Success/error message shown after submit.

### ES-152 — Build Admin Users and CompanyAdmins Management Page

- `/admin/users` page for Admins.
- Paginated table of all system users (10 per page).
- Columns: name, email, role, status, email verification, company, created date.
- Search by name or email, filter by role.
- Route protected; only Admin can access it.

### ES-153 — Add Create CompanyAdmin Form for Admin

- Create CompanyAdmin form added to `/admin/users`.
- Fields: first name, last name, email, temporary password, company dropdown.
- Company dropdown loaded from `GET /api/companies` (active only).
- New CompanyAdmin appears in the user list after creation.

### ES-154 — Add Activate and Deactivate User UI

- Activate/Deactivate button added to each user row in `/admin/users`.
- Deactivation requires confirmation via a modal dialog.
- Backend revokes active sessions when a user is deactivated.
- Status badge updates immediately after the action.
- Users cannot deactivate themselves.

### ES-155 — Implement Role-Based Dashboard Summary Endpoint

- `GET /api/dashboard/me` added to `DashboardController`.
- Role dispatch: Admin > CompanyAdmin > Agent > User.
- Admin: global property counts, user/company/agent totals, recent properties.
- CompanyAdmin: company-scoped property counts, agent count, recent company properties.
- Agent: agent-scoped property counts, recent agent properties.
- User: available property count, latest properties, popular cities.
- Separate DTOs per role. Business logic in `DashboardService`.

### ES-156 — Implement Redis Caching for Role-Based Dashboard Statistics

- Dashboard responses cached in Redis per role and scope.
- Cache keys: `dashboard:admin:global`, `dashboard:companyadmin:company:{companyId}`, `dashboard:agent:{agentId}`, `dashboard:user:marketplace`.
- TTL: 30 minutes as a fallback.
- Cache miss: query SQL, store result in Redis.
- Cache hit: return from Redis without hitting SQL.
- Redis unavailable: graceful fallback to SQL, no error surfaced to the client.
- `IDashboardCacheService` wraps `IRedisCacheService` with exception handling.

### ES-157 — Add Dashboard Cache Invalidation on Data Changes

- `IDashboardInvalidationService` with four scoped invalidation methods.
- Property created/updated/deleted: invalidates admin, company, agent, and user marketplace caches.
- Agent created/status changed: invalidates admin and company caches.
- CompanyAdmin created/status changed: invalidates admin and company caches.
- Regular User status changed: invalidates admin cache.
- Invalidation called from service layer (`PropertyService`, `UserService`), not controllers.
- Property update that changes agent or company invalidates both old and new scopes.

### ES-158 — Build Role-Based Dashboard UI

- `DashboardPage` rebuilt to call `GET /api/dashboard/me`.
- Role-specific views: AdminView, CompanyAdminView, AgentView, UserView.
- Each view shows metric cards, shortcut navigation cards, and a recent properties table.
- Loading, error, and empty states handled.

### ES-159 — Add Route Protection for Role-Specific Pages

- `ProtectedRoute` extended to accept `requiredRoles` and `requiredPermissions`.
- All Sprint 5 routes protected:
  - `/dashboard`: requires authentication.
  - `/my-properties`: requires Agent role and CreateProperty permission.
  - `/company/agents`: requires CompanyAdmin role and ManageAgents permission.
  - `/admin/users`: requires Admin role and ManageUsers permission.
  - `/properties/new`: requires CreateProperty permission.
  - `/properties/:id/edit`: requires EditProperty permission.

### ES-160 — Improve Shared Loading, Empty and Error States

- Three shared components created: `LoadingState`, `EmptyState`, `ErrorState`.
- Applied consistently across: PropertiesPage, CompanyAgentsPage, AdminUsersPage, MapPage, PropertyDetailsPage, DashboardPage.
- All pages now use the same `table-state` / `table-state-error` pattern.

## Key Files Added or Changed

### Backend

- `Controllers/DashboardController.cs`
- `Interfaces/IDashboardService.cs`
- `Interfaces/IDashboardCacheService.cs`
- `Interfaces/IDashboardInvalidationService.cs`
- `Interfaces/IAgentRepository.cs` (new method: `GetCompanyIdByUserIdAsync`)
- `Services/DashboardService.cs`
- `Services/DashboardCacheService.cs`
- `Services/DashboardInvalidationService.cs`
- `Services/PropertyService.cs` (invalidation calls)
- `Services/UserService.cs` (invalidation calls + IAgentRepository injection)
- `Services/RedisCacheService.cs` (DeleteAsync)
- `Repositories/AgentRepository.cs` (GetCompanyIdByUserIdAsync)
- `Constants/DashboardCacheKeys.cs`
- `DTOs/Dashboard/` (role-specific dashboard DTOs)

### Frontend

- `pages/RootEntryPage.tsx` (auth landing, redirect logic)
- `pages/LoginPage.tsx` (redesigned)
- `pages/RegisterPage.tsx` (redesigned)
- `pages/VerifyEmailPage.tsx` (redesigned)
- `pages/DashboardPage.tsx` (role-based views)
- `pages/PropertiesPage.tsx` (card grid, map link)
- `pages/CreatePropertyPage.tsx` (dedicated create route)
- `pages/MapPage.tsx` (full map search, sidebar, list link)
- `pages/CompanyAgentsPage.tsx` (agent management + create form)
- `pages/AdminUsersPage.tsx` (user management + create CompanyAdmin form + activate/deactivate)
- `pages/MyPropertiesPage.tsx`
- `components/properties/PropertyCard.tsx`
- `components/LoadingState.tsx`
- `components/EmptyState.tsx`
- `components/ErrorState.tsx`
- `components/ProtectedRoute.tsx` (requiredRoles, requiredPermissions)
- `routes/AppRouter.tsx` (all Sprint 5 routes)
- `services/api.ts` (dashboard types and function, updateUserStatus, getMyCompanyAgents)
- `layouts/AppLayout.tsx` / `components/Navbar.tsx` / `components/Sidebar.tsx` (role-aware navigation)
- `styles.css` (card grid, map layout, dashboard metrics, state components, auth pages)

### Tests

- `DashboardControllerTests.cs` (role dispatch, Redis cache hit/miss, fallback, key isolation)
- `DashboardCacheInvalidationTests.cs` (all 4 invalidation scopes, 9 tests)
- `PropertyServiceTests.cs` (invalidation on create/update/delete)

## Verification

```text
dotnet test
Result: 188/188 passing

dotnet build --configuration Release
Result: passed, 0 warnings, 0 errors

npm run build
Result: passed
```

## Known Gaps After Sprint 5

- No real SMTP email delivery; verification remains simulated.
- No forgot password or reset password flow.
- No cloud file storage; images are stored locally.
- No image ordering, cover image selection, or cropping.
- No frontend automated test suite.
- No production deployment configuration.
