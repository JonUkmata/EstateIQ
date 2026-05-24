using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs.Dashboard;
using EstateIQ.Extensions;
using EstateIQ.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EstateIQ.Services;

public class DashboardService(AppDbContext dbContext) : IDashboardService
{
    private readonly AppDbContext _dbContext = dbContext;

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
        var statusNames = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
            .Select(p => p.PropertyStatus.Name)
            .ToListAsync();

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
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

        return new AdminDashboardDto
        {
            TotalProperties = statusNames.Count,
            ForSaleProperties = statusNames.Count(n => n == "For Sale"),
            ForRentProperties = statusNames.Count(n => n == "For Rent"),
            SoldProperties = statusNames.Count(n => n == "Sold"),
            RentedProperties = statusNames.Count(n => n == "Rented"),
            TotalUsers = await _dbContext.Users.CountAsync(),
            TotalCompanies = await _dbContext.Companies.CountAsync(c => c.IsActive),
            TotalAgents = await _dbContext.Agents.CountAsync(a => a.IsActive),
            RecentProperties = recentProperties
        };
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

        var statusNames = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
            .Where(p => p.CompanyId == companyId)
            .Select(p => p.PropertyStatus.Name)
            .ToListAsync();

        var agentCount = await _dbContext.AgentCompanies
            .AsNoTracking()
            .CountAsync(ac => ac.CompanyId == companyId && ac.IsActive);

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
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

        return new CompanyAdminDashboardDto
        {
            CompanyId = companyId,
            CompanyName = companyUser.Company.Name,
            CompanyProperties = statusNames.Count,
            CompanyAgents = agentCount,
            ForSaleProperties = statusNames.Count(n => n == "For Sale"),
            ForRentProperties = statusNames.Count(n => n == "For Rent"),
            SoldProperties = statusNames.Count(n => n == "Sold"),
            RentedProperties = statusNames.Count(n => n == "Rented"),
            RecentCompanyProperties = recentProperties
        };
    }

    private async Task<AgentDashboardDto> GetAgentDashboardAsync(Guid userId)
    {
        var agent = await _dbContext.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (agent is null)
            return new AgentDashboardDto();

        var statusNames = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
            .Where(p => p.AgentId == agent.Id)
            .Select(p => p.PropertyStatus.Name)
            .ToListAsync();

        var recentProperties = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
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

        return new AgentDashboardDto
        {
            AgentId = agent.Id,
            MyProperties = statusNames.Count,
            MyForSaleProperties = statusNames.Count(n => n == "For Sale"),
            MyForRentProperties = statusNames.Count(n => n == "For Rent"),
            MySoldProperties = statusNames.Count(n => n == "Sold"),
            MyRentedProperties = statusNames.Count(n => n == "Rented"),
            RecentMyProperties = recentProperties
        };
    }

    private async Task<UserDashboardDto> GetUserDashboardAsync()
    {
        var availableCount = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
            .CountAsync(p => p.PropertyStatus.Name == "For Sale" || p.PropertyStatus.Name == "For Rent");

        var latestProperties = await _dbContext.Properties
            .AsNoTracking()
            .Include(p => p.PropertyStatus)
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

        return new UserDashboardDto
        {
            AvailableProperties = availableCount,
            LatestProperties = latestProperties,
            PopularCities = popularCities
        };
    }
}
