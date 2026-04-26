using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines business operations for property status lookups.
/// </summary>
public interface IPropertyStatusService
{
    /// <summary>
    /// Gets property statuses for dropdown usage.
    /// </summary>
    Task<IEnumerable<PropertyStatusDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null);
}
