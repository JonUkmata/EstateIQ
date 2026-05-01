# Sprint 3 - Property Discovery And Map Experience

## Sprint Goal

Complete the property discovery workflow by adding filtered/paginated listings, property details, edit/delete workflows, and a map view powered by persisted latitude and longitude values.

## Sprint Status

**Status:** Completed  
**Primary Theme:** Property discovery, map visualization, edit/delete workflow, branch stabilization  
**Result:** Users can search, filter, page through, inspect, edit, delete, and map properties from the frontend.

## What Was Delivered

### Backend Property API Expansion

- `GET /api/properties` now supports query parameters for filtering and pagination.
- `PUT /api/properties/{id}` updates existing properties.
- `DELETE /api/properties/{id}` deletes properties when business rules allow it.
- API responses return paged result metadata:
  - `items`
  - `totalCount`
  - `page`
  - `pageSize`
  - `totalPages`

### Query And Filtering Support

Supported property list query parameters:

- `search`
- `city`
- `propertyTypeId`
- `propertyStatusId`
- `minPrice`
- `maxPrice`
- `page`
- `pageSize`

Validation rules:

- `page` must be greater than zero.
- `pageSize` must be between 1 and 100.
- `minPrice` and `maxPrice` cannot be negative.
- `minPrice` cannot be greater than `maxPrice`.

### Frontend Properties Page

- Search input with debounce.
- City, property type, status, minimum price, and maximum price filters.
- Server-backed pagination.
- Create property form retained and integrated with refreshed paged results.
- Delete action with confirmation dialog.
- Links to details and edit pages.
- Loading, error, empty, success, and delete feedback states.

### Property Details Page

- Route: `/properties/:id`
- Displays property overview, location, classification, company, and agent data.
- Links back to the property list and to the edit page.
- Handles invalid IDs, loading state, API errors, and missing records.

### Property Edit Page

- Route: `/properties/:id/edit`
- Loads existing property data and lookup dropdowns.
- Filters agents by selected company.
- Validates required fields and numeric ranges before submit.
- Calls `PUT /api/properties/{id}` and navigates back to details after a successful update.

### Map Page

- Route: `/map`
- Uses Leaflet and React Leaflet.
- Loads properties from `GET /api/properties?page=1&pageSize=100`.
- Shows markers only for properties with valid coordinates.
- Supports city, property type, and status filters.
- Includes a synchronized property list beside the map.
- Selecting a property focuses the marker and opens its popup.

### Stabilization Cleanup

- Removed the default ASP.NET Core template `/weatherforecast` endpoint.
- Updated `EstateIQ.http` to target the real `/api/test` health endpoint.
- Normalized the property details area unit display to `m2`.

## Key Files

### Backend

- `/backend/EstateIQ/Controllers/PropertiesController.cs`
- `/backend/EstateIQ/DTOs/PropertyQueryParameters.cs`
- `/backend/EstateIQ/DTOs/PagedResult.cs`
- `/backend/EstateIQ/DTOs/UpdatePropertyDto.cs`
- `/backend/EstateIQ/Repositories/PropertyRepository.cs`
- `/backend/EstateIQ/Services/PropertyService.cs`
- `/backend/EstateIQ/Program.cs`
- `/backend/EstateIQ/EstateIQ.http`

### Frontend

- `/frontend/src/pages/PropertiesPage.tsx`
- `/frontend/src/pages/PropertyDetailsPage.tsx`
- `/frontend/src/pages/EditPropertyPage.tsx`
- `/frontend/src/pages/MapPage.tsx`
- `/frontend/src/routes/AppRouter.tsx`
- `/frontend/src/services/api.ts`
- `/frontend/src/styles.css`
- `/frontend/package.json`

### Tests

- `/backend/EstateIQ.Tests/PropertyRepositoryTests.cs`
- `/backend/EstateIQ.Tests/PropertyServiceTests.cs`
- `/backend/EstateIQ.Tests/PropertiesControllerTests.cs`
- `/backend/EstateIQ.Tests/AgentsControllerTests.cs`
- `/backend/EstateIQ.Tests/CompaniesControllerTests.cs`
- `/backend/EstateIQ.Tests/PropertyTypesControllerTests.cs`
- `/backend/EstateIQ.Tests/PropertyStatusesControllerTests.cs`
- `/backend/EstateIQ.Tests/CompanySeederTests.cs`
- `/backend/EstateIQ.Tests/AgentCompanySeederTests.cs`

## Verification

Current verification status:

```text
dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj
Result: 59/59 passing

dotnet build backend\EstateIQ\EstateIQ.csproj --configuration Release
Result: passed, 0 warnings, 0 errors

npm run build
Result: passed
```

Manual verification scope:

- Property list loads from the API.
- Search and filters call the paginated backend endpoint.
- Pagination metadata is reflected in the UI.
- Property creation refreshes the list.
- Details page loads a selected property.
- Edit page updates a selected property.
- Delete flow uses confirmation and refreshes the list.
- Map page renders valid-coordinate properties as markers.

## Known Gaps After Sprint 3

- No authentication or authorization yet.
- No dedicated company/agent management pages yet.
- Dashboard page is still placeholder-level.
- No geocoding or map-based coordinate picker yet.
- Map currently loads up to 100 filtered properties per request.
- Frontend has production build verification but no dedicated automated frontend test suite yet.

## Sprint 4 Planning Handoff

Recommended Sprint 4 theme:

**Operational Hardening And Management Workflows**

Recommended next tickets:

1. Add authentication and role-aware authorization.
2. Build company and agent management pages.
3. Replace dashboard placeholders with real API-backed metrics.
4. Add frontend tests for properties, details, edit, delete, and map flows.
5. Add geocoding or map-based coordinate selection.
6. Prepare production deployment configuration.
