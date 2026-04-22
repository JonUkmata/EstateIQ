using EstateIQ.Data;
using EstateIQ.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for agents.
/// </summary>
public class AgentRepository(AppDbContext dbContext) : IAgentRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Checks whether an agent exists.
    /// </summary>
    public Task<bool> ExistsAsync(int id)
    {
        return _dbContext.Agents
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Checks whether an agent is active.
    /// </summary>
    public Task<bool> IsActiveAsync(int id)
    {
        return _dbContext.Agents
            .AsNoTracking()
            .AnyAsync(x => x.Id == id && x.IsActive);
    }
}
