using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines business operations for company lookups.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Gets companies for dropdown usage.
    /// </summary>
    Task<IEnumerable<CompanyDropdownDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null);
}
