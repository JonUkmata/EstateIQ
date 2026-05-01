using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines business operations for property management.
/// </summary>
public interface IPropertyService
{
    /// <summary>
    /// Gets all properties.
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetAllAsync();

    /// <summary>
    /// Gets a single property by identifier.
    /// </summary>
    Task<PropertyDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new property.
    /// </summary>
    Task<PropertyDto> CreateAsync(CreatePropertyDto dto);

    /// <summary>
    /// Updates an existing property.
    /// </summary>
    Task<PropertyDto> UpdateAsync(int id, UpdatePropertyDto dto);

    /// <summary>
    /// Deletes a property.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Gets properties filtered by city.
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetByCityAsync(string city);

    /// <summary>
    /// Gets properties filtered by price range.
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Gets properties filtered by property type.
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetByPropertyTypeAsync(int propertyTypeId);

    /// <summary>
    /// Gets properties filtered by status.
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetByStatusAsync(int statusId);

    /// <summary>
    /// Searches properties by keyword.
    /// </summary>
    Task<IEnumerable<PropertyDto>> SearchAsync(string keyword);

    /// <summary>
    /// Gets a paginated list of properties.
    /// </summary>
    Task<PagedResult<PropertyDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Gets a filtered and paginated list of properties.
    /// </summary>
    Task<PagedResult<PropertyDto>> GetFilteredAsync(PropertyQueryParameters queryParameters);

    /// <summary>
    /// Changes the status of a property.
    /// </summary>
    Task<PropertyDto> ChangeStatusAsync(int id, int newStatusId);

    /// <summary>
    /// Updates the price of a property.
    /// </summary>
    Task<PropertyDto> UpdatePriceAsync(int id, decimal newPrice);

    /// <summary>
    /// Evaluates whether a property can be deleted.
    /// </summary>
    Task<bool> CanDeleteAsync(int id);
}
