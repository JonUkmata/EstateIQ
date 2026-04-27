using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

/// <summary>
/// Provides Entity Framework Core data access operations for properties.
/// </summary>
public class PropertyRepository(AppDbContext dbContext) : IPropertyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all properties without related entities.
    /// </summary>
    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await _dbContext.Properties
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a property by identifier without related entities.
    /// </summary>
    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _dbContext.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Creates a new property record.
    /// </summary>
    public async Task<Property> CreateAsync(Property property)
    {
        ArgumentNullException.ThrowIfNull(property);

        await ValidateForeignKeysAsync(property);

        if (property.CreatedAt == default)
        {
            property.CreatedAt = DateTime.Now;
        }

        await _dbContext.Properties.AddAsync(property);
        await _dbContext.SaveChangesAsync();

        return property;
    }

    /// <summary>
    /// Updates an existing property record.
    /// </summary>
    public async Task<Property> UpdateAsync(Property property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var existingProperty = await _dbContext.Properties
            .FirstOrDefaultAsync(x => x.Id == property.Id);

        if (existingProperty is null)
        {
            throw new DbUpdateConcurrencyException($"Property with id {property.Id} was not found.");
        }

        await ValidateForeignKeysAsync(property);

        existingProperty.Title = property.Title;
        existingProperty.Description = property.Description;
        existingProperty.Price = property.Price;
        existingProperty.Area = property.Area;
        existingProperty.Bedrooms = property.Bedrooms;
        existingProperty.Bathrooms = property.Bathrooms;
        existingProperty.Floors = property.Floors;
        existingProperty.YearBuilt = property.YearBuilt;
        existingProperty.PropertyTypeId = property.PropertyTypeId;
        existingProperty.PropertyStatusId = property.PropertyStatusId;
        existingProperty.CompanyId = property.CompanyId;
        existingProperty.AgentId = property.AgentId;
        existingProperty.Address = property.Address;
        existingProperty.City = property.City;
        existingProperty.Latitude = property.Latitude;
        existingProperty.Longitude = property.Longitude;
        existingProperty.UpdatedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync();

        return existingProperty;
    }

    /// <summary>
    /// Deletes a property by identifier.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var property = await _dbContext.Properties
            .FirstOrDefaultAsync(x => x.Id == id);

        if (property is null)
        {
            return false;
        }

        _dbContext.Properties.Remove(property);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Gets a property by identifier with related entities included.
    /// </summary>
    public async Task<Property?> GetByIdWithDetailsAsync(int id)
    {
        return await CreateDetailedQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Gets all properties with related entities included.
    /// </summary>
    public async Task<IEnumerable<Property>> GetAllWithDetailsAsync()
    {
        return await CreateDetailedQuery()
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets properties filtered by city.
    /// </summary>
    public async Task<IEnumerable<Property>> GetByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return [];
        }

        var normalizedCity = city.Trim();

        return await _dbContext.Properties
            .AsNoTracking()
            .Where(x => x.City == normalizedCity)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets properties within the specified price range.
    /// </summary>
    public async Task<IEnumerable<Property>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.");
        }

        return await _dbContext.Properties
            .AsNoTracking()
            .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
            .OrderBy(x => x.Price)
            .ToListAsync();
    }

    /// <summary>
    /// Gets properties filtered by property type.
    /// </summary>
    public async Task<IEnumerable<Property>> GetByPropertyTypeAsync(int propertyTypeId)
    {
        return await _dbContext.Properties
            .AsNoTracking()
            .Where(x => x.PropertyTypeId == propertyTypeId)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets properties filtered by status.
    /// </summary>
    public async Task<IEnumerable<Property>> GetByStatusAsync(int statusId)
    {
        return await _dbContext.Properties
            .AsNoTracking()
            .Where(x => x.PropertyStatusId == statusId)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Searches properties by title or description.
    /// </summary>
    public async Task<IEnumerable<Property>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        var searchTerm = keyword.Trim();

        return await _dbContext.Properties
            .AsNoTracking()
            .Where(x =>
                x.Title.Contains(searchTerm) ||
                (x.Description != null && x.Description.Contains(searchTerm)))
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a page of properties with sorting support.
    /// </summary>
    public async Task<(IEnumerable<Property> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
        }

        IQueryable<Property> query = _dbContext.Properties.AsNoTracking();
        query = ApplySorting(query, sortBy, ascending);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Gets filtered properties with related entities included.
    /// </summary>
    public async Task<(IEnumerable<Property> Items, int TotalCount)> GetFilteredAsync(PropertyQueryParameters queryParameters)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        if (queryParameters.Page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queryParameters.Page), "Page number must be greater than zero.");
        }

        if (queryParameters.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queryParameters.PageSize), "Page size must be greater than zero.");
        }

        if (queryParameters.MinPrice.HasValue &&
            queryParameters.MaxPrice.HasValue &&
            queryParameters.MinPrice.Value > queryParameters.MaxPrice.Value)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.");
        }

        var query = ApplyFilters(CreateDetailedQuery(), queryParameters)
            .OrderBy(x => x.Id);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Checks whether a property exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Properties.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Gets the total number of properties.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _dbContext.Properties.CountAsync();
    }

    private IQueryable<Property> CreateDetailedQuery()
    {
        return _dbContext.Properties
            .AsNoTracking()
            .Include(x => x.Agent)
            .Include(x => x.Company)
            .Include(x => x.PropertyType)
            .Include(x => x.PropertyStatus);
    }

    private static IQueryable<Property> ApplyFilters(IQueryable<Property> query, PropertyQueryParameters queryParameters)
    {
        if (!string.IsNullOrWhiteSpace(queryParameters.City))
        {
            var city = queryParameters.City.Trim().ToLower();
            query = query.Where(x => x.City.ToLower() == city);
        }

        if (queryParameters.PropertyTypeId.HasValue)
        {
            query = query.Where(x => x.PropertyTypeId == queryParameters.PropertyTypeId.Value);
        }

        if (queryParameters.PropertyStatusId.HasValue)
        {
            query = query.Where(x => x.PropertyStatusId == queryParameters.PropertyStatusId.Value);
        }

        if (queryParameters.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= queryParameters.MinPrice.Value);
        }

        if (queryParameters.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= queryParameters.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            var searchTerm = queryParameters.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(searchTerm) ||
                (x.Description != null && x.Description.ToLower().Contains(searchTerm)));
        }

        return query;
    }

    private static IQueryable<Property> ApplySorting(IQueryable<Property> query, string? sortBy, bool ascending)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
            "price" => ascending ? query.OrderBy(x => x.Price) : query.OrderByDescending(x => x.Price),
            "city" => ascending ? query.OrderBy(x => x.City) : query.OrderByDescending(x => x.City),
            "area" => ascending ? query.OrderBy(x => x.Area) : query.OrderByDescending(x => x.Area),
            "yearbuilt" => ascending ? query.OrderBy(x => x.YearBuilt) : query.OrderByDescending(x => x.YearBuilt),
            "createdat" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            "updatedat" => ascending ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt),
            _ => ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
        };
    }

    private async Task ValidateForeignKeysAsync(Property property)
    {
        var propertyTypeExists = await _dbContext.PropertyTypes.AnyAsync(x => x.Id == property.PropertyTypeId);
        var propertyStatusExists = await _dbContext.PropertyStatuses.AnyAsync(x => x.Id == property.PropertyStatusId);
        var companyExists = await _dbContext.Companies.AnyAsync(x => x.Id == property.CompanyId);
        var agentExists = await _dbContext.Agents.AnyAsync(x => x.Id == property.AgentId);

        var missingReferences = new List<string>();

        if (!propertyTypeExists)
        {
            missingReferences.Add($"PropertyTypeId {property.PropertyTypeId}");
        }

        if (!propertyStatusExists)
        {
            missingReferences.Add($"PropertyStatusId {property.PropertyStatusId}");
        }

        if (!companyExists)
        {
            missingReferences.Add($"CompanyId {property.CompanyId}");
        }

        if (!agentExists)
        {
            missingReferences.Add($"AgentId {property.AgentId}");
        }

        if (missingReferences.Count > 0)
        {
            throw new ArgumentException($"Invalid foreign key reference(s): {string.Join(", ", missingReferences)}.");
        }
    }
}
