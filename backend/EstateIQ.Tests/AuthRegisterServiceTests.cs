using EstateIQ.Data;
using EstateIQ.DTOs.Auth;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class AuthRegisterServiceTests
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
    public async Task RegisterAsync_CreatesPublicUser()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var authService = CreateAuthService(dbContext);

        var response = await authService.RegisterAsync(BuildRequest());

        var user = await dbContext.Users.SingleAsync();
        Assert.Equal("jon@example.com", user.Email);
        Assert.Equal("Jon", user.FirstName);
        Assert.Equal("Ukmata", user.LastName);
        Assert.False(user.IsEmailConfirmed);
        Assert.True(user.IsActive);
        Assert.Equal("Registration successful. Please verify your email before logging in.", response.Message);
        Assert.False(string.IsNullOrWhiteSpace(response.VerificationToken));
        Assert.False(response.VerificationEmailSent);
    }

    [Fact]
    public async Task RegisterAsync_StoresHashedPassword()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var passwordService = new PasswordService();
        var authService = CreateAuthService(dbContext, passwordService);

        await authService.RegisterAsync(BuildRequest());

        var user = await dbContext.Users.SingleAsync();
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(passwordService.VerifyPassword(user, user.PasswordHash, "Password123!"));
    }

    [Fact]
    public async Task RegisterAsync_AssignsUserRole()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var authService = CreateAuthService(dbContext);

        await authService.RegisterAsync(BuildRequest());

        var userRole = await dbContext.UserRoles.SingleAsync();
        var role = await dbContext.Roles.SingleAsync(x => x.Id == userRole.RoleId);
        Assert.Equal("User", role.Name);
    }

    [Fact]
    public async Task RegisterAsync_CreatesEmailVerificationToken()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var authService = CreateAuthService(dbContext);

        var response = await authService.RegisterAsync(BuildRequest());

        var verificationToken = await dbContext.EmailVerificationTokens.SingleAsync();
        Assert.Equal(response.VerificationToken, verificationToken.Token);
        Assert.Null(verificationToken.UsedAt);
        Assert.True(verificationToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var authService = CreateAuthService(dbContext);
        await authService.RegisterAsync(BuildRequest());

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            authService.RegisterAsync(BuildRequest(email: " JON@example.com ")));

        Assert.Equal("Email is already registered.", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_RejectsInvalidPassword()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.RegisterAsync(BuildRequest(password: "password", confirmPassword: "password")));

        Assert.Contains(nameof(RegisterRequestDto.Password), exception.Errors.Keys);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext, IPasswordService? passwordService = null)
    {
        return new AuthService(
            new AuthRepository(dbContext),
            passwordService ?? new PasswordService(),
            new TokenService(Options.Create(TestJwtSettings)),
            new FakeEmailSender(),
            NullLogger<AuthService>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static RegisterRequestDto BuildRequest(
        string email = "jon@example.com",
        string password = "Password123!",
        string confirmPassword = "Password123!")
    {
        return new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };
    }
}
