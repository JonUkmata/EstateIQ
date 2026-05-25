# ML Prediction API Contract

This document is kept for historical context. The active implementation is documented in:

- [ML price prediction flow](ml-price-prediction-flow.md)

## Current EstateIQ Endpoint

The frontend calls the EstateIQ backend, not the ML service directly:

```http
POST /api/properties/generate-price
```

Access:

- Requires `CreateProperty` permission.

The backend validates the agent-facing form data, converts area units to square feet, fills defaults, derives technical ML fields, calls the FastAPI service, and returns a suggested listing price.

## Current FastAPI ML Endpoint

The backend calls:

```http
POST http://127.0.0.1:8000/predict
```

Request shape:

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

Response shape:

```json
{
  "predicted_price": 464627.78,
  "predicted_price_formatted": "$464,628"
}
```

## Historical Draft

An earlier draft proposed:

```http
POST /api/ml/predict-price
```

with a smaller request body containing `bedrooms`, `bathrooms`, `sqft`, and `zipcode`, and a response containing `predictedPrice` plus `confidence`.

That draft is no longer the active implementation. The project now uses the backend-mediated property endpoint and the King County style ML request shape described above.
