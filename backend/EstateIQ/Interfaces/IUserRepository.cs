using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IUserRepository
{
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> CompanyExistsAsync(int companyId);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task AddCompanyAdminAsync(User user, UserRole userRole, CompanyUser companyUser);

    Task<bool> CompanyUserExistsAsync(Guid userId, int companyId, string relationshipType);

    Task AddAgentUserAsync(User user, UserRole userRole, Agent agent, AgentCompany agentCompany);
}
