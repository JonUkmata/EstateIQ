using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core lookup operations for companies.
/// </summary>
public class CompanyRepository(AppDbContext dbContext) : ICompanyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all active companies sorted by name.
    /// </summary>
    public async Task<IEnumerable<Company>> GetAllActiveAsync()
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.IsActive)
            .OrderBy(company => company.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all companies sorted by name.
    /// </summary>
    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Searches companies by name and returns results sorted by name.
    /// </summary>
    public async Task<IEnumerable<Company>> SearchByNameAsync(string searchTerm)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Name.Contains(searchTerm))
            .OrderBy(company => company.Name)
            .ToListAsync();
    }

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
