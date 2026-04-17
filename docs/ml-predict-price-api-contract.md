# ML Prediction API Contract

This document defines the initial API contract for the ML price prediction feature in EstateIQ.

## Goal

Expose a single endpoint that accepts basic property inputs and returns a predicted property price together with a confidence score.

## Endpoint

- Method: `POST`
- Path: `/api/ml/predict-price`
- Content-Type: `application/json`

## Request Contract

All fields are required in the initial version of this contract.

### Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `bedrooms` | `int` | Yes | Number of bedrooms |
| `bathrooms` | `int` | Yes | Number of bathrooms |
| `sqft` | `float` | Yes | Property size in square feet |
| `zipcode` | `string` | Yes | Property ZIP code |

### Example Request

```json
{
  "bedrooms": 3,
  "bathrooms": 2,
  "sqft": 1450.5,
  "zipcode": "10001"
}
```

## Response Contract

### Success Response

- Status: `200 OK`

### Response Body

| Field | Type | Description |
| --- | --- | --- |
| `predictedPrice` | `float` | Predicted property price |
| `confidence` | `float` | Confidence score from `0.0` to `1.0` |

### Example Response

```json
{
  "predictedPrice": 327500.0,
  "confidence": 0.87
}
```

## Validation Rules

- `bedrooms` must be an integer
- `bathrooms` must be an integer
- `sqft` must be a numeric value
- `zipcode` must be a string
- `confidence` must always be returned in the range `0.0` to `1.0`

## Error Handling

For the first contract draft, invalid or missing request data should return:

- Status: `400 Bad Request`

Example cases:

- Missing required field
- Wrong data type in request body
- Empty JSON body

## Out of Scope

This document only defines the API contract.

- No ML model implementation
- No prediction algorithm definition
- No persistence or analytics behavior
