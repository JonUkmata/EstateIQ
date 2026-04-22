using EstateIQ.Models;

namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for property statuses.
/// </summary>
public interface IPropertyStatusRepository
{
    /// <summary>
    /// Checks whether a property status exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets a property status by identifier.
    /// </summary>
    Task<PropertyStatus?> GetByIdAsync(int id);
}
