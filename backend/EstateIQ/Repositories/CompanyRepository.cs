using EstateIQ.Data;
using EstateIQ.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for companies.
/// </summary>
public class CompanyRepository(AppDbContext dbContext) : ICompanyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Checks whether a company exists.
    /// </summary>
    public Task<bool> ExistsAsync(int id)
    {
        return _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Checks whether a company is active.
    /// </summary>
    public Task<bool> IsActiveAsync(int id)
    {
        return _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(x => x.Id == id && x.IsActive);
    }
}
