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
