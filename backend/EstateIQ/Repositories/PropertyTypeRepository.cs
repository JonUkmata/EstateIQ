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
    /// Checks whether a property type exists.
    /// </summary>
    public Task<bool> ExistsAsync(int id)
    {
        return _dbContext.PropertyTypes
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);
    }
}
