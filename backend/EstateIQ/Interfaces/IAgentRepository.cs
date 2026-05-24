namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for agents.
/// </summary>
public interface IAgentRepository
{
    /// <summary>
    /// Gets all active agents sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Agent>> GetAllActiveAsync();

    /// <summary>
    /// Gets all agents sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Agent>> GetAllAsync();

    /// <summary>
    /// Searches agents by first name, last name, or email and returns results sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Agent>> SearchAsync(string searchTerm);

    /// <summary>
    /// Gets active agents assigned to a company sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Agent>> GetActiveByCompanyAsync(int companyId);

    /// <summary>
    /// Gets active agents assigned to companies managed by the specified company admin user.
    /// </summary>
    Task<IEnumerable<Models.Agent>> GetActiveByCompanyAdminAsync(Guid companyAdminUserId);

    /// <summary>
    /// Checks whether an agent exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Checks whether an agent is active.
    /// </summary>
    Task<bool> IsActiveAsync(int id);
}
