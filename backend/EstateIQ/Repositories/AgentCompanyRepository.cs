using EstateIQ.Data;
using EstateIQ.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for agent-company relationships.
/// </summary>
public class AgentCompanyRepository(AppDbContext dbContext) : IAgentCompanyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Checks whether an active relationship exists between an agent and company.
    /// </summary>
    public Task<bool> ExistsActiveRelationshipAsync(int agentId, int companyId)
    {
        return _dbContext.AgentCompanies
            .AsNoTracking()
            .AnyAsync(x => x.AgentId == agentId && x.CompanyId == companyId && x.IsActive);
    }
}
