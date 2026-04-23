
# Real Estate Management System

## Project Overview

The Real Estate Management System is a full-stack web application designed to streamline the creation, management, and visualization of property listings. The platform will provide users with powerful analytics dashboards and interactive map-based property visualization. In future iterations, the system will integrate advanced Machine Learning models to predict property prices based on various features, enhancing decision-making for users.

## Technology Stack

- **Backend:** ASP.NET Core Web API (.NET)
- **Frontend:** React (Node.js)
- **Database:** PostgreSQL (preferred) or SQL Server
- **Architecture:** Monolithic layered architecture

## Project Goals

- Enable users to create, update, and manage property listings efficiently
- Provide analytics dashboards for property and market insights
- Visualize properties on interactive maps
- Ensure a scalable, maintainable, and secure codebase
- Lay the groundwork for future ML-driven property price prediction

## Project Structure (Planned)

```
backend/   - ASP.NET Core Web API (Backend logic and API)
frontend/  - React application (Frontend UI)
docs/      - Documentation and resources
ml/        - Machine Learning models and scripts
```

*Note: The folder structure now reflects the main project directories. Add or update folders as the project evolves.*

## Future Machine Learning Integration

A dedicated module will be developed to integrate Machine Learning models for property price prediction. This will involve:
- Data collection and preprocessing
- Model training and evaluation
- API endpoints for ML predictions
- Integration with the main application

*Details and implementation roadmap will be added in future releases.*

## API Notes

### Company Dropdown Endpoint

`GET /api/companies`

Query parameters:
- `includeInactive` optional boolean, defaults to `false`
- `search` optional string filter applied to company name

Example response:

```json
[
  {
    "id": 1,
    "name": "ABC Real Estate",
    "city": "Prishtinë",
    "isActive": true
  }
]
```

Behavior:
- Returns `200 OK` with an array, including `[]` when no companies match
- Returns only active companies by default
- Sorts results alphabetically by company name
- Returns a lightweight payload intended for frontend dropdowns
