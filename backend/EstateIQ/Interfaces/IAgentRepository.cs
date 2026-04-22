namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for agents.
/// </summary>
public interface IAgentRepository
{
    /// <summary>
    /// Checks whether an agent exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Checks whether an agent is active.
    /// </summary>
    Task<bool> IsActiveAsync(int id);
}
