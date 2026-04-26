# Sprint 2 - Property Management And Seed Data

## Sprint Goal

Build the first usable property management workflow: seed core lookup data, expose dropdown APIs, create property API endpoints, display properties in the frontend, and create new properties from the UI.

## Sprint Status

**Status:** Completed  
**Primary Theme:** Property module, lookup data, frontend create/list flow  
**Result:** Properties can be created through the frontend, persisted through the API, listed after submit, and returned with coordinates for future map work.

## What Was Delivered

### Seed Data

- Property type seed data.
- Property status seed data.
- Company seed data.
- Agent seed data.
- Agent-company relationship seed data.
- Demo property seed data when the property table is empty.

### Backend Lookup APIs

- `GET /api/propertytypes`
- `GET /api/propertystatuses`
- `GET /api/companies`
- `GET /api/agents`
- `GET /api/agents?companyId={id}`

These endpoints support dropdowns used by the property form.

### Backend Property APIs

- `GET /api/properties`
- `GET /api/properties/{id}`
- `POST /api/properties`

The properties API returns related lookup data:

- property type
- property status
- company
- agent

### Property Domain Layer

- `Property` model.
- `CreatePropertyDto`, `UpdatePropertyDto`, and `PropertyDto`.
- `PropertyRepository` with CRUD, detail includes, filtering, searching, paging, and sorting support.
- `PropertyService` with validation and business rules.
- Custom exceptions:
  - `ValidationException`
  - `NotFoundException`
  - `BusinessRuleException`

### Business Rules

- Property type must exist.
- Property status must exist.
- Company must exist and be active.
- Agent must exist and be active.
- Agent must have an active relationship with the selected company.
- Sold properties cannot have price changed.
- Sold/rented properties cannot be deleted.
- Rented properties protect structural and assignment fields from updates.

### Frontend Property Page

- `/properties` page.
- Property list table.
- Shows:
  - title
  - price
  - city
  - status
- Loading, error, and empty states.
- Responsive table layout.

### Frontend Create Form

- Text inputs:
  - title
  - city
  - address
  - description
- Dropdowns:
  - property type
  - property status
  - company
  - agent
- Numeric fields:
  - price
  - area
  - bedrooms
  - bathrooms
  - floors
  - year built
  - latitude
  - longitude
- Frontend validations for required fields and numeric ranges.
- Submit calls `POST /api/properties`.
- After successful submit, the list refreshes from `GET /api/properties`.
- Agent dropdown filters by selected company.

### Map Readiness

Latitude and longitude are now part of the property creation flow and API response.

Coordinate rules:

- Latitude: `-90` to `90`
- Longitude: `-180` to `180`

This is the critical handoff point for Sprint 3 map visualization.

## Key Files

### Backend

- `/backend/EstateIQ/Controllers/PropertiesController.cs`
- `/backend/EstateIQ/Controllers/AgentsController.cs`
- `/backend/EstateIQ/Controllers/CompaniesController.cs`
- `/backend/EstateIQ/Controllers/PropertyTypesController.cs`
- `/backend/EstateIQ/Controllers/PropertyStatusesController.cs`
- `/backend/EstateIQ/Models/Property.cs`
- `/backend/EstateIQ/Models/PropertyType.cs`
- `/backend/EstateIQ/Models/PropertyStatus.cs`
- `/backend/EstateIQ/Models/Company.cs`
- `/backend/EstateIQ/Models/Agent.cs`
- `/backend/EstateIQ/Models/AgentCompany.cs`
- `/backend/EstateIQ/DTOs/CreatePropertyDto.cs`
- `/backend/EstateIQ/DTOs/PropertyDto.cs`
- `/backend/EstateIQ/Repositories/PropertyRepository.cs`
- `/backend/EstateIQ/Services/PropertyService.cs`
- `/backend/EstateIQ/Data/PropertyTypeSeeder.cs`
- `/backend/EstateIQ/Data/PropertyStatusSeeder.cs`
- `/backend/EstateIQ/Data/CompanySeeder.cs`
- `/backend/EstateIQ/Data/AgentCompanySeeder.cs`
- `/backend/EstateIQ/Data/PropertySeeder.cs`

### Frontend

- `/frontend/src/pages/PropertiesPage.tsx`
- `/frontend/src/services/api.ts`
- `/frontend/src/styles.css`

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
Result: 49/49 passing

npm run build
Result: passed
```

Manual verification completed:

- `GET /api/properties` returns properties.
- `POST /api/properties` saves a property.
- Property list refreshes after submit.
- Coordinates are persisted and returned by API.

Example coordinate verification:

```json
{
  "CreatedId": 6,
  "Latitude": 41.32750000,
  "Longitude": 19.81870000
}
```

## Known Gaps After Sprint 2

- No map view yet.
- No frontend property details page yet.
- No frontend edit/delete property workflow yet.
- Property filters are not fully exposed through the public controller query API.
- No geocoding or map-based coordinate picker yet.
- No authentication/authorization yet.

## Sprint 3 Planning Handoff

Recommended Sprint 3 theme:

**Property Discovery And Map Experience**

Recommended next tickets:

1. Add query filters to `GET /api/properties`.
2. Add filter UI on `/properties`.
3. Add property details page.
4. Add map view using persisted latitude/longitude.
5. Connect list, filters, details, and map markers.
6. Add edit property workflow if sprint capacity allows.

