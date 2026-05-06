using System.Security.Claims;
using EstateIQ.Constants;

namespace EstateIQ.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin);
    }

    public static bool IsCompanyAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.CompanyAdmin);
    }

    public static bool CanManageAgentsForCompany(this ClaimsPrincipal user, int companyId)
    {
        if (user.IsAdmin())
        {
            return true;
        }

        var companyIdClaim = user.FindFirstValue("companyId");

        return user.IsCompanyAdmin()
            && int.TryParse(companyIdClaim, out var assignedCompanyId)
            && assignedCompanyId == companyId;
    }
}
