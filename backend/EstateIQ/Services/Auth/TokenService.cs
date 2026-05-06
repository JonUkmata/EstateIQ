using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EstateIQ.Services.Auth;

public class TokenService : ITokenService
{
    private const int RefreshTokenByteLength = 64;
    private const int VerificationTokenByteLength = 32;
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
        ValidateJwtSettings(_jwtSettings);
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Distinct(StringComparer.Ordinal).Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Distinct(StringComparer.Ordinal).Select(permission => new Claim("permission", permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwtSettings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return GenerateSecureToken(RefreshTokenByteLength);
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    public string GenerateVerificationToken()
    {
        return GenerateSecureToken(VerificationTokenByteLength);
    }

    public DateTime GetAccessTokenExpirationUtc()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);
    }

    public DateTime GetRefreshTokenExpirationUtc()
    {
        return DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);
    }

    private static string GenerateSecureToken(int byteLength)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(byteLength);

        return Convert.ToBase64String(randomBytes);
    }

    private static void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
        {
            throw new InvalidOperationException("JWT key must be at least 32 bytes long.");
        }

        if (jwtSettings.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("JWT access token lifetime must be greater than zero.");
        }

        if (jwtSettings.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("JWT refresh token lifetime must be greater than zero.");
        }
    }
}
