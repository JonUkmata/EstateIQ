using EstateIQ.DTOs;
using EstateIQ.Models;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines data access operations for property entities.
/// </summary>
public interface IPropertyRepository
{
    /// <summary>
    /// Gets all properties without related entities.
    /// </summary>
    Task<IEnumerable<Property>> GetAllAsync();

    /// <summary>
    /// Gets a property by identifier without related entities.
    /// </summary>
    Task<Property?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new property record.
    /// </summary>
    Task<Property> CreateAsync(Property property);

    /// <summary>
    /// Updates an existing property record.
    /// </summary>
    Task<Property> UpdateAsync(Property property);

    /// <summary>
    /// Deletes a property by identifier.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Gets a property by identifier with related entities included.
    /// </summary>
    Task<Property?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Gets all properties with related entities included.
    /// </summary>
    Task<IEnumerable<Property>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets properties filtered by city.
    /// </summary>
    Task<IEnumerable<Property>> GetByCityAsync(string city);

    /// <summary>
    /// Gets properties within the specified price range.
    /// </summary>
    Task<IEnumerable<Property>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Gets properties filtered by property type.
    /// </summary>
    Task<IEnumerable<Property>> GetByPropertyTypeAsync(int propertyTypeId);

    /// <summary>
    /// Gets properties filtered by status.
    /// </summary>
    Task<IEnumerable<Property>> GetByStatusAsync(int statusId);

    /// <summary>
    /// Searches properties by title or description.
    /// </summary>
    Task<IEnumerable<Property>> SearchAsync(string keyword);

    /// <summary>
    /// Gets a page of properties with sorting support.
    /// </summary>
    Task<(IEnumerable<Property> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Gets a filtered and paginated list of properties with related entities included.
    /// </summary>
    Task<(IEnumerable<Property> Items, int TotalCount)> GetFilteredAsync(PropertyQueryParameters queryParameters);

    /// <summary>
    /// Checks whether a property exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total number of properties.
    /// </summary>
    Task<int> GetCountAsync();
}
