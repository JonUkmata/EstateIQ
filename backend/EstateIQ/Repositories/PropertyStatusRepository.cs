using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for property statuses.
/// </summary>
public class PropertyStatusRepository(AppDbContext dbContext) : IPropertyStatusRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all active property statuses sorted by name.
    /// </summary>
    public async Task<IEnumerable<PropertyStatus>> GetAllActiveAsync()
    {
        return await _dbContext.PropertyStatuses
            .AsNoTracking()
            .Where(status => status.IsActive)
            .OrderBy(status => status.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all property statuses sorted by name.
    /// </summary>
    public async Task<IEnumerable<PropertyStatus>> GetAllAsync()
    {
        return await _dbContext.PropertyStatuses
            .AsNoTracking()
            .OrderBy(status => status.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Searches property statuses by name and returns results sorted by name.
    /// </summary>
    public async Task<IEnumerable<PropertyStatus>> SearchByNameAsync(string searchTerm)
    {
        return await _dbContext.PropertyStatuses
            .AsNoTracking()
            .Where(status => status.Name.Contains(searchTerm))
            .OrderBy(status => status.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks whether a property status exists.
    /// </summary>
    public Task<bool> ExistsAsync(int id)
    {
        return _dbContext.PropertyStatuses
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Gets a property status by identifier.
    /// </summary>
    public Task<PropertyStatus?> GetByIdAsync(int id)
    {
        return _dbContext.PropertyStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
