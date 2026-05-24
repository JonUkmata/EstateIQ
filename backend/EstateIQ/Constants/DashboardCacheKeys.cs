namespace EstateIQ.Constants;

public static class DashboardCacheKeys
{
    public const string AdminGlobal = "dashboard:admin:global";
    public const string UserMarketplace = "dashboard:user:marketplace";

    public static string CompanyAdmin(int companyId) => $"dashboard:companyadmin:company:{companyId}";
    public static string Agent(int agentId) => $"dashboard:agent:{agentId}";
}
