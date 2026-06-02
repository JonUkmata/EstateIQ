using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs.Dashboard;
using EstateIQ.Extensions;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EstateIQ.Services;

public class DashboardService(AppDbContext dbContext, IDashboardCacheService cache) : IDashboardService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IDashboardCacheService _cache = cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public async Task<object> GetDashboardAsync(Guid userId, ClaimsPrincipal principal)
    {
        if (principal.IsAdmin())
            return await GetAdminDashboardAsync();

        if (principal.IsCompanyAdmin())
            return await GetCompanyAdminDashboardAsync(userId);

        if (principal.IsInRole(Roles.Agent))
            return await GetAgentDashboardAsync(userId);

        return await GetUserDashboardAsync();
    }

    private async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        const string key = DashboardCacheKeys.AdminGlobal;
        var cached = await _cache.GetAsync<AdminDashboardDto>(key);
        if (cached is not null) return cached;

        var statusCounts = await GetStatusCountsAsync(_dbContext.Properties);

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new DashboardPropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                Status = p.PropertyStatus.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new AdminDashboardDto
        {
            TotalProperties = statusCounts.Values.Sum(),
            ForSaleProperties = GetStatusCount(statusCounts, "For Sale"),
            ForRentProperties = GetStatusCount(statusCounts, "For Rent"),
            SoldProperties = GetStatusCount(statusCounts, "Sold"),
            RentedProperties = GetStatusCount(statusCounts, "Rented"),
            TotalUsers = await _dbContext.Users.CountAsync(),
            TotalCompanies = await _dbContext.Companies.CountAsync(c => c.IsActive),
            TotalAgents = await _dbContext.Agents.CountAsync(a => a.IsActive),
            RecentProperties = recentProperties
        };

        await _cache.SetAsync(key, result, CacheTtl);
        return result;
    }

    private async Task<CompanyAdminDashboardDto> GetCompanyAdminDashboardAsync(Guid userId)
    {
        var companyUser = await _dbContext.CompanyUsers
            .AsNoTracking()
            .Include(cu => cu.Company)
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.RelationshipType == Roles.CompanyAdmin);

        if (companyUser is null)
            return new CompanyAdminDashboardDto();

        var companyId = companyUser.CompanyId;
        var key = DashboardCacheKeys.CompanyAdmin(companyId);
        var cached = await _cache.GetAsync<CompanyAdminDashboardDto>(key);
        if (cached is not null) return cached;

        var statusCounts = await GetStatusCountsAsync(_dbContext.Properties.Where(p => p.CompanyId == companyId));

        var agentCount = await _dbContext.AgentCompanies
            .AsNoTracking()
            .CountAsync(ac => ac.CompanyId == companyId && ac.IsActive);

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new DashboardPropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                Status = p.PropertyStatus.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new CompanyAdminDashboardDto
        {
            CompanyId = companyId,
            CompanyName = companyUser.Company.Name,
            CompanyProperties = statusCounts.Values.Sum(),
            CompanyAgents = agentCount,
            ForSaleProperties = GetStatusCount(statusCounts, "For Sale"),
            ForRentProperties = GetStatusCount(statusCounts, "For Rent"),
            SoldProperties = GetStatusCount(statusCounts, "Sold"),
            RentedProperties = GetStatusCount(statusCounts, "Rented"),
            RecentCompanyProperties = recentProperties
        };

        await _cache.SetAsync(key, result, CacheTtl);
        return result;
    }

    private async Task<AgentDashboardDto> GetAgentDashboardAsync(Guid userId)
    {
        var agent = await _dbContext.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (agent is null)
            return new AgentDashboardDto();

        var key = DashboardCacheKeys.Agent(agent.Id);
        var cached = await _cache.GetAsync<AgentDashboardDto>(key);
        if (cached is not null) return cached;

        var statusCounts = await GetStatusCountsAsync(_dbContext.Properties.Where(p => p.AgentId == agent.Id));

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .Where(p => p.AgentId == agent.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new DashboardPropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                Status = p.PropertyStatus.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new AgentDashboardDto
        {
            AgentId = agent.Id,
            MyProperties = statusCounts.Values.Sum(),
            MyForSaleProperties = GetStatusCount(statusCounts, "For Sale"),
            MyForRentProperties = GetStatusCount(statusCounts, "For Rent"),
            MySoldProperties = GetStatusCount(statusCounts, "Sold"),
            MyRentedProperties = GetStatusCount(statusCounts, "Rented"),
            RecentMyProperties = recentProperties
        };

        await _cache.SetAsync(key, result, CacheTtl);
        return result;
    }

    private async Task<UserDashboardDto> GetUserDashboardAsync()
    {
        const string key = DashboardCacheKeys.UserMarketplace;
        var cached = await _cache.GetAsync<UserDashboardDto>(key);
        if (cached is not null) return cached;

        var availableCount = await _dbContext.Properties
            .AsNoTracking()
            .CountAsync(p => p.PropertyStatus.Name == "For Sale" || p.PropertyStatus.Name == "For Rent");

        var latestProperties = await _dbContext.Properties
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new DashboardPropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                Status = p.PropertyStatus.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var popularCities = await _dbContext.Properties
            .AsNoTracking()
            .GroupBy(p => p.City)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToListAsync();

        var result = new UserDashboardDto
        {
            AvailableProperties = availableCount,
            LatestProperties = latestProperties,
            PopularCities = popularCities
        };

        await _cache.SetAsync(key, result, CacheTtl);
        return result;
    }

    private static async Task<Dictionary<string, int>> GetStatusCountsAsync(IQueryable<Property> properties)
    {
        return await properties
            .AsNoTracking()
            .GroupBy(p => p.PropertyStatus.Name)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    private static int GetStatusCount(IReadOnlyDictionary<string, int> statusCounts, string status)
    {
        return statusCounts.TryGetValue(status, out var count) ? count : 0;
    }
}
