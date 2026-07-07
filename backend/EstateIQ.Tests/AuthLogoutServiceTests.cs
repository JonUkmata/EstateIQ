using EstateIQ.Data;
using EstateIQ.DTOs.Auth;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class AuthLogoutServiceTests
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
    public async Task LogoutAsync_ValidRefreshToken_RevokesToken()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext);
        var refreshToken = await SeedRefreshTokenAsync(dbContext, tokenService, user);
        var authService = CreateAuthService(dbContext, tokenService);

        var response = await authService.LogoutAsync(new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        });

        var storedRefreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.Equal("Logged out successfully.", response.Message);
        Assert.NotNull(storedRefreshToken.RevokedAt);
    }

    [Fact]
    public async Task LogoutAsync_InvalidOrMissingRefreshToken_ReturnsSuccess()
    {
        await using var dbContext = CreateContext();
        var authService = CreateAuthService(dbContext, CreateTokenService());

        var missingResponse = await authService.LogoutAsync(new RefreshTokenRequestDto());
        var invalidResponse = await authService.LogoutAsync(new RefreshTokenRequestDto
        {
            RefreshToken = "not-a-real-refresh-token"
        });

        Assert.Equal("Logged out successfully.", missingResponse.Message);
        Assert.Equal("Logged out successfully.", invalidResponse.Message);
    }

    [Fact]
    public async Task LogoutAsync_AlreadyRevokedRefreshToken_ReturnsSuccessWithoutChangingRevokedAt()
    {
        await using var dbContext = CreateContext();
        var tokenService = CreateTokenService();
        var user = await SeedUserAsync(dbContext);
        var revokedAt = DateTime.UtcNow.AddMinutes(-5);
        var refreshToken = await SeedRefreshTokenAsync(dbContext, tokenService, user, revokedAt);
        var authService = CreateAuthService(dbContext, tokenService);

        var response = await authService.LogoutAsync(new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        });

        var storedRefreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.Equal("Logged out successfully.", response.Message);
        Assert.Equal(revokedAt, storedRefreshToken.RevokedAt);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext, TokenService tokenService)
    {
        return new AuthService(
            new AuthRepository(dbContext),
            new PasswordService(),
            tokenService,
            new FakeEmailSender(),
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

    private static async Task<User> SeedUserAsync(AppDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            IsEmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    private static async Task<string> SeedRefreshTokenAsync(
        AppDbContext dbContext,
        TokenService tokenService,
        User user,
        DateTime? revokedAt = null)
    {
        var refreshToken = tokenService.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = revokedAt,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return refreshToken;
    }
}
