namespace EstateIQ.Interfaces;

public interface IDashboardInvalidationService
{
    Task InvalidateDashboardsForPropertyChangeAsync(int companyId, int agentId);
    Task InvalidateDashboardsForAgentChangeAsync(int companyId);
    Task InvalidateDashboardsForCompanyAdminChangeAsync(int companyId);
    Task InvalidateDashboardsForUserChangeAsync();
}
