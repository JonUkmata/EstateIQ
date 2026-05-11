using System.Security.Claims;

namespace EstateIQ.Tests;

public static class ClaimsPrincipalFactory
{
    public static ClaimsPrincipal Create(string role, string? companyId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrWhiteSpace(companyId))
        {
            claims.Add(new Claim("companyId", companyId));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
