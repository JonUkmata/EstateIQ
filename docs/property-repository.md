# Property Repository

`PropertyRepository` provides CRUD, filtering, search, pagination, and eager-loading operations for `Property`.

## Registration

The repository is registered in dependency injection in [Program.cs](../backend/EstateIQ/Program.cs):

```csharp
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
```

## Main Methods

- `GetAllAsync()` returns all properties
- `GetByIdAsync(int id)` returns one property or `null`
- `CreateAsync(Property property)` inserts a new property
- `UpdateAsync(Property property)` updates an existing property and sets `UpdatedAt`
- `DeleteAsync(int id)` performs hard delete and returns `false` if the property does not exist
- `GetByIdWithDetailsAsync(int id)` and `GetAllWithDetailsAsync()` include `Agent`, `Company`, `PropertyType`, and `PropertyStatus`
- `GetByCityAsync`, `GetByPriceRangeAsync`, `GetByPropertyTypeAsync`, `GetByStatusAsync`, and `SearchAsync` support filtering
- `GetPagedAsync(...)` supports pagination and sorting

## Example

```csharp
var property = await propertyRepository.GetByIdWithDetailsAsync(id);

var paged = await propertyRepository.GetPagedAsync(
    pageNumber: 1,
    pageSize: 10,
    sortBy: "price",
    ascending: false);
```

## Error Scenarios

- `GetByIdAsync` returns `null` when the record does not exist
- `DeleteAsync` returns `false` when the record does not exist
- `UpdateAsync` throws `DbUpdateConcurrencyException` when the record does not exist
- `CreateAsync` and `UpdateAsync` throw `ArgumentException` when one or more foreign keys reference missing records
