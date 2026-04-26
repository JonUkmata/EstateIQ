using EstateIQ.Data;
using EstateIQ.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for property types.
/// </summary>
public class PropertyTypeRepository(AppDbContext dbContext) : IPropertyTypeRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all active property types sorted by name.
    /// </summary>
    public async Task<IEnumerable<Models.PropertyType>> GetAllActiveAsync()
    {
        return await _dbContext.PropertyTypes
            .AsNoTracking()
            .Where(propertyType => propertyType.IsActive)
            .OrderBy(propertyType => propertyType.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all property types sorted by name.
    /// </summary>
    public async Task<IEnumerable<Models.PropertyType>> GetAllAsync()
    {
        return await _dbContext.PropertyTypes
            .AsNoTracking()
            .OrderBy(propertyType => propertyType.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Searches property types by name and returns results sorted by name.
    /// </summary>
    public async Task<IEnumerable<Models.PropertyType>> SearchByNameAsync(string searchTerm)
    {
        return await _dbContext.PropertyTypes
            .AsNoTracking()
            .Where(propertyType => propertyType.Name.Contains(searchTerm))
            .OrderBy(propertyType => propertyType.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks whether a property type exists.
    /// </summary>
    public Task<bool> ExistsAsync(int id)
    {
        return _dbContext.PropertyTypes
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);
    }
}
