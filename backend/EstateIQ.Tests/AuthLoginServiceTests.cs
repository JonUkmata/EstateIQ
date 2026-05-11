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

public class AuthLoginServiceTests
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
    public async Task LoginAsync_FailsBeforeEmailVerification()
    {
        await using var dbContext = CreateContext();
        await SeedUserAsync(dbContext, isEmailConfirmed: false, isActive: true);
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.LoginAsync(BuildRequest()));

        Assert.Equal("Email is not verified.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_SucceedsAfterEmailVerification()
    {
        await using var dbContext = CreateContext();
        var user = await SeedUserAsync(dbContext, isEmailConfirmed: true, isActive: true);
        var authService = CreateAuthService(dbContext);

        var response = await authService.LoginAsync(BuildRequest());

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal("jon@example.com", response.User.Email);
        Assert.Equal(["User"], response.User.Roles);
        Assert.Equal(["BookViewing", "ViewProperties"], response.User.Permissions.Order().ToArray());
    }

    [Fact]
    public async Task LoginAsync_InvalidPasswordFails()
    {
        await using var dbContext = CreateContext();
        await SeedUserAsync(dbContext, isEmailConfirmed: true, isActive: true);
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.LoginAsync(BuildRequest(password: "WrongPassword123!")));

        Assert.Contains("Email or password is invalid.", exception.Errors[nameof(LoginRequestDto.Email)]);
    }

    [Fact]
    public async Task LoginAsync_InactiveUserFails()
    {
        await using var dbContext = CreateContext();
        await SeedUserAsync(dbContext, isEmailConfirmed: true, isActive: false);
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.LoginAsync(BuildRequest()));

        Assert.Equal("Account is inactive.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_StoresRefreshTokenHashOnly()
    {
        await using var dbContext = CreateContext();
        await SeedUserAsync(dbContext, isEmailConfirmed: true, isActive: true);
        var tokenService = CreateTokenService();
        var authService = CreateAuthService(dbContext, tokenService: tokenService);

        var response = await authService.LoginAsync(BuildRequest());

        var refreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotEqual(response.RefreshToken, refreshToken.TokenHash);
        Assert.Equal(tokenService.HashToken(response.RefreshToken), refreshToken.TokenHash);
        Assert.Null(refreshToken.RevokedAt);
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext, TokenService? tokenService = null)
    {
        return new AuthService(
            new AuthRepository(dbContext),
            new PasswordService(),
            tokenService ?? CreateTokenService(),
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

    private static async Task<User> SeedUserAsync(AppDbContext dbContext, bool isEmailConfirmed, bool isActive)
    {
        await dbContext.Database.EnsureCreatedAsync();
        var passwordService = new PasswordService();
        var userRole = await dbContext.Roles.SingleAsync(role => role.Name == "User");
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            IsEmailConfirmed = isEmailConfirmed,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordService.HashPassword(user, "Password123!");

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

    private static LoginRequestDto BuildRequest(string password = "Password123!")
    {
        return new LoginRequestDto
        {
            Email = "jon@example.com",
            Password = password
        };
    }
}
