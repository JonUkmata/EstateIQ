using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for agents.
/// </summary>
public class AgentRepository(AppDbContext dbContext) : IAgentRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all active agents sorted by name.
    /// </summary>
    public async Task<IEnumerable<Agent>> GetAllActiveAsync()
    {
        return await CreateSortedQuery()
            .Where(agent => agent.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all agents sorted by name.
    /// </summary>
    public async Task<IEnumerable<Agent>> GetAllAsync()
    {
        return await CreateSortedQuery()
            .ToListAsync();
    }

    /// <summary>
    /// Searches agents by first name, last name, or email and returns results sorted by name.
    /// </summary>
    public async Task<IEnumerable<Agent>> SearchAsync(string searchTerm)
    {
        var normalizedSearch = searchTerm.Trim();

        return await CreateSortedQuery()
            .Where(agent =>
                agent.FirstName.Contains(normalizedSearch) ||
                agent.LastName.Contains(normalizedSearch) ||
                agent.Email.Contains(normalizedSearch))
            .ToListAsync();
    }

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

    private IQueryable<Agent> CreateSortedQuery()
    {
        return _dbContext.Agents
            .AsNoTracking()
            .OrderBy(agent => agent.FirstName)
            .ThenBy(agent => agent.LastName)
            .ThenBy(agent => agent.Email);
    }
}
