using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        string? search,
        string? role,
        int page,
        int pageSize)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Include(user => user.CompanyUsers)
                .ThenInclude(companyUser => companyUser.Company)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(user =>
                user.FirstName.Contains(normalizedSearch) ||
                user.LastName.Contains(normalizedSearch) ||
                user.Email.Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.Trim();
            query = query.Where(user => user.UserRoles.Any(userRole => userRole.Role.Name == normalizedRole));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == email);
    }

    public Task<bool> CompanyExistsAsync(int companyId)
    {
        return _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(company => company.Id == companyId);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(role => role.Name == roleName);
    }

    public async Task AddCompanyAdminAsync(User user, UserRole userRole, CompanyUser companyUser)
    {
        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);
        _dbContext.CompanyUsers.Add(companyUser);

        await _dbContext.SaveChangesAsync();
    }

    public Task<bool> CompanyUserExistsAsync(Guid userId, int companyId, string relationshipType)
    {
        return _dbContext.CompanyUsers
            .AsNoTracking()
            .AnyAsync(companyUser =>
                companyUser.UserId == userId &&
                companyUser.CompanyId == companyId &&
                companyUser.RelationshipType == relationshipType);
    }

    public Task<int?> GetCompanyIdForUserRelationshipAsync(Guid userId, string relationshipType)
    {
        return _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.UserId == userId &&
                companyUser.RelationshipType == relationshipType)
            .Select(companyUser => (int?)companyUser.CompanyId)
            .FirstOrDefaultAsync();
    }

    public async Task AddAgentUserAsync(User user, UserRole userRole, Agent agent, AgentCompany agentCompany)
    {
        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);
        _dbContext.Agents.Add(agent);
        _dbContext.AgentCompanies.Add(agentCompany);

        await _dbContext.SaveChangesAsync();
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == id);
    }

    public async Task UpdateUserStatusAsync(User user, bool revokeRefreshTokens)
    {
        _dbContext.Users.Update(user);

        if (revokeRefreshTokens)
        {
            var now = DateTime.UtcNow;
            var activeRefreshTokens = await _dbContext.RefreshTokens
                .Where(refreshToken =>
                    refreshToken.UserId == user.Id &&
                    refreshToken.RevokedAt == null &&
                    refreshToken.ExpiresAt > now)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.RevokedAt = now;
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
