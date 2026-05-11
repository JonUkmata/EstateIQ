using EstateIQ.Data;
using EstateIQ.DTOs.Auth;
using EstateIQ.Exceptions;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class AuthRefreshServiceTests
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
    public async Task RefreshAsync_ValidRefreshToken_ReturnsNewAccessToken()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext, isActive: true);
        var refreshToken = await SeedRefreshTokenAsync(dbContext, tokenService, user, expiresAt: DateTime.UtcNow.AddDays(1));
        var authService = CreateAuthService(dbContext, tokenService);

        var response = await authService.RefreshAsync(new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        });

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshAsync_RevokedRefreshToken_Fails()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext, isActive: true);
        var refreshToken = await SeedRefreshTokenAsync(
            dbContext,
            tokenService,
            user,
            expiresAt: DateTime.UtcNow.AddDays(1),
            revokedAt: DateTime.UtcNow);
        var authService = CreateAuthService(dbContext, tokenService);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.RefreshAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            }));

        Assert.Equal("Refresh token is invalid.", exception.Message);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredRefreshToken_Fails()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext, isActive: true);
        var refreshToken = await SeedRefreshTokenAsync(dbContext, tokenService, user, expiresAt: DateTime.UtcNow.AddMinutes(-1));
        var authService = CreateAuthService(dbContext, tokenService);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.RefreshAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            }));

        Assert.Equal("Refresh token is invalid.", exception.Message);
    }

    [Fact]
    public async Task RefreshAsync_InactiveUser_Fails()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext, isActive: false);
        var refreshToken = await SeedRefreshTokenAsync(dbContext, tokenService, user, expiresAt: DateTime.UtcNow.AddDays(1));
        var authService = CreateAuthService(dbContext, tokenService);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.RefreshAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            }));

        Assert.Equal("Refresh token is invalid.", exception.Message);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext, TokenService tokenService)
    {
        return new AuthService(
            new AuthRepository(dbContext),
            new PasswordService(),
            tokenService,
            NullLogger<AuthService>.Instance);
    }

    private static TokenService CreateTokenService()
    {
        return new TokenService(Options.Create(TestJwtSettings));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<User> SeedUserAsync(AppDbContext dbContext, bool isActive)
    {
        await dbContext.Database.EnsureCreatedAsync();
        var userRole = await dbContext.Roles.SingleAsync(role => role.Name == "User");
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            IsEmailConfirmed = true,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = userRole.Id,
            AssignedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return user;
    }

    private static async Task<string> SeedRefreshTokenAsync(
        AppDbContext dbContext,
        TokenService tokenService,
        User user,
        DateTime expiresAt,
        DateTime? revokedAt = null)
    {
        var refreshToken = tokenService.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return refreshToken;
    }
}
