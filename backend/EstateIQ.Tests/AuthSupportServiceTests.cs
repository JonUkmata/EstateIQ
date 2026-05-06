using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EstateIQ.Models;
using EstateIQ.Services.Auth;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class AuthSupportServiceTests
{
    private static readonly JwtSettings TestJwtSettings = new()
    {
        Issuer = "EstateIQ.Tests",
        Audience = "EstateIQ.Tests",
        Key = "EstateIQ-Tests-Jwt-Key-Minimum-32-Bytes-2026",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var passwordService = new PasswordService();
        var user = BuildUser();
        var passwordHash = passwordService.HashPassword(user, "CorrectPassword123!");

        var result = passwordService.VerifyPassword(user, passwordHash, "CorrectPassword123!");

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var passwordService = new PasswordService();
        var user = BuildUser();
        var passwordHash = passwordService.HashPassword(user, "CorrectPassword123!");

        var result = passwordService.VerifyPassword(user, passwordHash, "WrongPassword123!");

        Assert.False(result);
    }

    [Fact]
    public void HashToken_ReturnsDeterministicHash()
    {
        var tokenService = CreateTokenService();
        const string refreshToken = "refresh-token-value";

        var firstHash = tokenService.HashToken(refreshToken);
        var secondHash = tokenService.HashToken(refreshToken);

        Assert.Equal(firstHash, secondHash);
        Assert.NotEqual(refreshToken, firstHash);
        Assert.False(string.IsNullOrWhiteSpace(firstHash));
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyJwtWithUserRolesAndPermissions()
    {
        var tokenService = CreateTokenService();
        var user = BuildUser();

        var accessToken = tokenService.GenerateAccessToken(
            user,
            roles: ["Admin"],
            permissions: ["ManageUsers", "ViewProperties"]);

        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        Assert.Equal(TestJwtSettings.Issuer, jwt.Issuer);
        Assert.Contains(TestJwtSettings.Audience, jwt.Audiences);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(jwt.Claims, claim => claim.Type == "permission" && claim.Value == "ManageUsers");
        Assert.Contains(jwt.Claims, claim => claim.Type == "permission" && claim.Value == "ViewProperties");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentNonEmptyValues()
    {
        var tokenService = CreateTokenService();

        var firstToken = tokenService.GenerateRefreshToken();
        var secondToken = tokenService.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(firstToken));
        Assert.False(string.IsNullOrWhiteSpace(secondToken));
        Assert.NotEqual(firstToken, secondToken);
    }

    [Fact]
    public void GenerateVerificationToken_ReturnsNonEmptyValue()
    {
        var tokenService = CreateTokenService();

        var verificationToken = tokenService.GenerateVerificationToken();

        Assert.False(string.IsNullOrWhiteSpace(verificationToken));
    }

    private static TokenService CreateTokenService()
    {
        return new TokenService(Options.Create(TestJwtSettings));
    }

    private static User BuildUser()
    {
        return new User
        {
            Id = Guid.Parse("f0d1c2b3-a4e5-4678-9012-3456789abcde"),
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@estateiq.local",
            PasswordHash = "not-used"
        };
    }
}
