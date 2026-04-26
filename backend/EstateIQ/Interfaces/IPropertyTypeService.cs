using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines business operations for property type lookups.
/// </summary>
public interface IPropertyTypeService
{
    /// <summary>
    /// Gets property types for dropdown usage.
    /// </summary>
    Task<IEnumerable<PropertyTypeDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null);
}
