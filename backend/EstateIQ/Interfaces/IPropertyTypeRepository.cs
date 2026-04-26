namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for property types.
/// </summary>
public interface IPropertyTypeRepository
{
    /// <summary>
    /// Gets all active property types sorted by name.
    /// </summary>
    Task<IEnumerable<Models.PropertyType>> GetAllActiveAsync();

    /// <summary>
    /// Gets all property types sorted by name.
    /// </summary>
    Task<IEnumerable<Models.PropertyType>> GetAllAsync();

    /// <summary>
    /// Searches property types by name and returns results sorted by name.
    /// </summary>
    Task<IEnumerable<Models.PropertyType>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks whether a property type exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
