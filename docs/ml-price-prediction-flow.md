# ML Price Prediction Flow

This document describes the current EstateIQ ML price prediction integration for the New Property form.

## Goal

Agents should create a normal property listing, not a technical ML request. The frontend collects agent-friendly fields, the backend translates those fields into the ML service contract, and the ML service returns a suggested price that the agent can apply or ignore.

## Runtime Architecture

```text
New Property form
  -> EstateIQ backend: POST /api/properties/generate-price
  -> FastAPI ML service: POST http://127.0.0.1:8000/predict
  -> EstateIQ backend response
  -> Generate Price card in the frontend
```

The frontend must not call the FastAPI ML service directly.

## Frontend Flow

The New Property form includes a separate highlighted Generate Price card. The agent fills the listing fields, property details, and optional accuracy details, then clicks `Generate Price`.

The frontend sends the current form data to:

```http
POST /api/properties/generate-price
```

The generated price is shown as a suggestion. The agent can click `Apply to Price`, but the final listing price remains manually editable.

## Form Sections

The form is organized into:

- Listing Info
- Location
- Property Details
- Optional Accuracy Details
- Generate Price
- Final Price

Area fields default to `sqft`. Agents can switch area fields to `m2` where needed.

## Location Behavior

The Location section includes a Leaflet map picker and manual coordinate fields.

When the agent clicks the map, the frontend updates:

- Latitude
- Longitude
- Address
- City
- Zipcode

When the agent manually edits Latitude or Longitude, the frontend uses the same reverse-geocoding behavior after a short debounce and updates Address, City, and Zipcode when lookup data is available.

## Backend Endpoint

```http
POST /api/properties/generate-price
```

Access:

- Requires `CreateProperty` permission.

Response:

```json
{
  "suggestedPrice": 464627.78,
  "formattedPrice": "$464,628",
  "mlWarnings": []
}
```

## Backend Input DTO

The backend receives agent-facing fields, not raw ML fields:

```json
{
  "livingArea": 1960,
  "livingAreaUnit": "sqft",
  "bedrooms": 4,
  "bathrooms": 3,
  "floors": 1,
  "yearBuilt": 1965,
  "latitude": 47.520801,
  "longitude": -122.393001,
  "zipcode": 98136,
  "lotArea": 5000,
  "lotAreaUnit": "sqft",
  "condition": 5,
  "grade": 7,
  "hasBasement": true,
  "basementArea": 910,
  "basementAreaUnit": "sqft",
  "waterfront": false,
  "viewQuality": 0,
  "renovated": false,
  "yearRenovated": null,
  "nearbyLivingArea": 1360,
  "nearbyLivingAreaUnit": "sqft",
  "nearbyLotArea": 5000,
  "nearbyLotAreaUnit": "sqft"
}
```

## ML Service Contract

The FastAPI service runs at:

```http
POST http://127.0.0.1:8000/predict
```

The backend sends this JSON shape:

```json
{
  "bedrooms": 4,
  "bathrooms": 3,
  "sqft_living": 1960,
  "sqft_lot": 5000,
  "floors": 1,
  "waterfront": 0,
  "view": 0,
  "condition": 5,
  "grade": 7,
  "sqft_above": 1050,
  "sqft_basement": 910,
  "yr_built": 1965,
  "yr_renovated": 0,
  "zipcode": 98136,
  "lat": 47.520801,
  "long": -122.393001,
  "sqft_living15": 1360,
  "sqft_lot15": 5000,
  "sale_year": 2014,
  "sale_month": 11
}
```

Expected ML response:

```json
{
  "predicted_price": 464627.78,
  "predicted_price_formatted": "$464,628"
}
```

## Field Mapping

| Agent-facing field | Backend / ML field | Notes |
| --- | --- | --- |
| `livingArea` | `sqft_living` | Converted to sqft if unit is `m2` |
| `lotArea` | `sqft_lot` | Defaults to `sqft_living` |
| `bedrooms` | `bedrooms` | Required |
| `bathrooms` | `bathrooms` | Required |
| `floors` | `floors` | Required |
| `yearBuilt` | `yr_built` | Required |
| `renovated`, `yearRenovated` | `yr_renovated` | Defaults to `0` when not renovated |
| `zipcode` | `zipcode` | Required |
| `latitude` | `lat` | Required |
| `longitude` | `long` | Required |
| `condition` | `condition` | Defaults to `3` |
| `grade` | `grade` | Defaults to `7` |
| `waterfront` | `waterfront` | Boolean converted to `0` or `1` |
| `viewQuality` | `view` | Defaults to `0` |
| `basementArea` | `sqft_basement` | Defaults to `0` |
| `nearbyLivingArea` | `sqft_living15` | Defaults to `sqft_living` |
| `nearbyLotArea` | `sqft_lot15` | Defaults to `sqft_lot` |
| backend constant | `sale_year` | Always `2014` |
| backend constant | `sale_month` | Always `11` |

## Unit Conversion

All area values sent to the ML model are in square feet.

```text
if unit == m2:
  sqft = round(value * 10.7639)

if unit == sqft:
  sqft = round(value)
```

Converted fields:

- Living Area
- Lot Area
- Basement Area
- Nearby Living Area
- Nearby Lot Area

## Derived Fields

The backend derives:

```text
sqft_above = sqft_living - sqft_basement
```

Example:

```text
sqft_living = 1960
sqft_basement = 910
sqft_above = 1050
```

If basement area is larger than living area, the backend returns a validation error.

## Defaults

When optional values are missing:

```text
waterfront = 0
view = 0
condition = 3
grade = 7
sqft_basement = 0
yr_renovated = 0
sqft_lot = sqft_living
sqft_above = sqft_living - sqft_basement
sqft_living15 = sqft_living
sqft_lot15 = sqft_lot
sale_year = 2014
sale_month = 11
```

## Hard Validation

The backend blocks invalid requests for:

- Living Area less than or equal to zero
- Bedrooms outside `1-10`
- Bathrooms outside `0.5-8`
- Floors outside `1-3.5`
- Year Built outside the business range
- Latitude outside `-90` to `90`
- Longitude outside `-180` to `180`
- Basement Area greater than Living Area
- Year Renovated before Year Built
- Year Renovated after the current year
- Condition outside `1-5`
- Quality Grade outside `1-13`
- View Quality outside `0-4`

## Soft Warnings

The backend can return ML awareness warnings without blocking the request:

- Location is outside the model training area.
- Zipcode is outside the model training area.
- Year built is outside the model training range.

These warnings are displayed politely in the Generate Price card.

## Relevant Files

Backend:

- `backend/EstateIQ/Controllers/PropertiesController.cs`
- `backend/EstateIQ/DTOs/GeneratePropertyPriceRequestDto.cs`
- `backend/EstateIQ/DTOs/GeneratePropertyPriceResponseDto.cs`
- `backend/EstateIQ/Interfaces/IPropertyPricePredictionService.cs`
- `backend/EstateIQ/Services/PropertyPricePredictionService.cs`

Frontend:

- `frontend/src/pages/CreatePropertyPage.tsx`
- `frontend/src/services/api.ts`
- `frontend/src/styles.css`

Tests:

- `backend/EstateIQ.Tests/PropertyPricePredictionServiceTests.cs`

## Local Testing

Start the ML service:

```powershell
# In the ML service project
uvicorn app:app --host 127.0.0.1 --port 8000
```

Start the EstateIQ backend:

```powershell
dotnet run --project .\backend\EstateIQ\EstateIQ.csproj --launch-profile http
```

Start the frontend:

```powershell
cd .\frontend
npm run dev
```

Open:

```text
http://localhost:5173/properties/new
```

Fill the required property details and click `Generate Price`.
