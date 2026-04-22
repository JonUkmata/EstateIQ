namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for agent-company relationships.
/// </summary>
public interface IAgentCompanyRepository
{
    /// <summary>
    /// Checks whether an active relationship exists between an agent and company.
    /// </summary>
    Task<bool> ExistsActiveRelationshipAsync(int agentId, int companyId);
}
