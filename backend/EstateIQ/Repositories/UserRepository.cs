using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(user =>
                user.FirstName.Contains(normalizedSearch) ||
                user.LastName.Contains(normalizedSearch) ||
                user.Email.Contains(normalizedSearch));
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
}
