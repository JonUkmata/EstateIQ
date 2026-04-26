using EstateIQ.Models;

namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for property statuses.
/// </summary>
public interface IPropertyStatusRepository
{
    /// <summary>
    /// Gets all active property statuses sorted by name.
    /// </summary>
    Task<IEnumerable<PropertyStatus>> GetAllActiveAsync();

    /// <summary>
    /// Gets all property statuses sorted by name.
    /// </summary>
    Task<IEnumerable<PropertyStatus>> GetAllAsync();

    /// <summary>
    /// Searches property statuses by name and returns results sorted by name.
    /// </summary>
    Task<IEnumerable<PropertyStatus>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks whether a property status exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets a property status by identifier.
    /// </summary>
    Task<PropertyStatus?> GetByIdAsync(int id);
}
