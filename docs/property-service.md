# Property Service

`PropertyService` implements the business logic layer for the property module and sits between controllers and repositories.

## Responsibilities

- Validates DTO payloads with data annotations and service-level rules
- Validates foreign key references through repository abstractions
- Enforces business rules such as:
  `Agent` must belong to `Company`
  sold properties cannot change price
  sold or rented properties cannot be deleted
- Maps entities to DTOs through AutoMapper
- Logs create, update, delete, and error scenarios

## Main Components

- Service interface:
  [IPropertyService.cs](../backend/EstateIQ/Interfaces/IPropertyService.cs)
- Service implementation:
  [PropertyService.cs](../backend/EstateIQ/Services/PropertyService.cs)
- Mapping profile:
  [MappingProfile.cs](../backend/EstateIQ/Mappings/MappingProfile.cs)
- Exceptions:
  [NotFoundException.cs](../backend/EstateIQ/Exceptions/NotFoundException.cs)
  [ValidationException.cs](../backend/EstateIQ/Exceptions/ValidationException.cs)
  [BusinessRuleException.cs](../backend/EstateIQ/Exceptions/BusinessRuleException.cs)

## Example

```csharp
var created = await propertyService.CreateAsync(new CreatePropertyDto
{
    Title = "Modern Apartment",
    Price = 120000m,
    Area = 78m,
    PropertyTypeId = 1,
    PropertyStatusId = 1,
    CompanyId = 1,
    AgentId = 1,
    Address = "Rruga e Kavajes",
    City = "Tirane"
});
```

## Error Scenarios

- `NotFoundException` when a requested property does not exist
- `ValidationException` when DTO or reference validation fails
- `BusinessRuleException` when domain rules are violated
