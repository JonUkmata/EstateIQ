using EstateIQ.Constants;
using EstateIQ.Interfaces;

namespace EstateIQ.Services;

public class DashboardInvalidationService(IDashboardCacheService cache) : IDashboardInvalidationService
{
    public async Task InvalidateDashboardsForPropertyChangeAsync(int companyId, int agentId)
    {
        await cache.DeleteAsync(DashboardCacheKeys.AdminGlobal);
        await cache.DeleteAsync(DashboardCacheKeys.CompanyAdmin(companyId));
        await cache.DeleteAsync(DashboardCacheKeys.Agent(agentId));
        await cache.DeleteAsync(DashboardCacheKeys.UserMarketplace);
    }

    public async Task InvalidateDashboardsForAgentChangeAsync(int companyId)
    {
        await cache.DeleteAsync(DashboardCacheKeys.AdminGlobal);
        await cache.DeleteAsync(DashboardCacheKeys.CompanyAdmin(companyId));
    }

    public async Task InvalidateDashboardsForCompanyAdminChangeAsync(int companyId)
    {
        await cache.DeleteAsync(DashboardCacheKeys.AdminGlobal);
        await cache.DeleteAsync(DashboardCacheKeys.CompanyAdmin(companyId));
    }

    public Task InvalidateDashboardsForUserChangeAsync()
    {
        return cache.DeleteAsync(DashboardCacheKeys.AdminGlobal);
    }
}
