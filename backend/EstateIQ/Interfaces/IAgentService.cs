using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines business operations for agent lookups.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Gets agents for dropdown usage.
    /// </summary>
    Task<IEnumerable<AgentDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null,
        int? companyId = null);
}
