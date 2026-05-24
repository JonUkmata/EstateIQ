using System.Security.Claims;

namespace EstateIQ.Interfaces;

/// <summary>
/// Provides role-scoped dashboard summary data for the current user.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns a dashboard summary appropriate for the caller's role.
    /// Role precedence: Admin > CompanyAdmin > Agent > User.
    /// </summary>
    Task<object> GetDashboardAsync(Guid userId, ClaimsPrincipal principal);
}
