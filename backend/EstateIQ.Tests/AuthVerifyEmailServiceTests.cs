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

public class AuthVerifyEmailServiceTests
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
    public async Task VerifyEmailAsync_ValidTokenConfirmsUserAndMarksTokenUsed()
    {
        await using var dbContext = CreateContext();
        await SeedUnverifiedUserWithTokenAsync(dbContext, "valid-token", DateTime.UtcNow.AddHours(1));
        var authService = CreateAuthService(dbContext);

        var response = await authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "valid-token" });

        var user = await dbContext.Users.SingleAsync();
        var token = await dbContext.EmailVerificationTokens.SingleAsync();
        Assert.Equal("Email verified successfully. You can now login.", response.Message);
        Assert.True(user.IsEmailConfirmed);
        Assert.NotNull(user.UpdatedAt);
        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidTokenThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        await SeedUnverifiedUserWithTokenAsync(dbContext, "valid-token", DateTime.UtcNow.AddHours(1));
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "invalid-token" }));

        Assert.Contains(nameof(VerifyEmailRequestDto.Token), exception.Errors.Keys);
        Assert.Contains("Verification token is invalid.", exception.Errors[nameof(VerifyEmailRequestDto.Token)]);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredTokenThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        await SeedUnverifiedUserWithTokenAsync(dbContext, "expired-token", DateTime.UtcNow.AddMinutes(-1));
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "expired-token" }));

        Assert.Contains("Verification token has expired.", exception.Errors[nameof(VerifyEmailRequestDto.Token)]);
    }

    [Fact]
    public async Task VerifyEmailAsync_UsedTokenCannotBeReused()
    {
        await using var dbContext = CreateContext();
        await SeedUnverifiedUserWithTokenAsync(dbContext, "used-token", DateTime.UtcNow.AddHours(1));
        var authService = CreateAuthService(dbContext);
        await authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "used-token" });

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "used-token" }));

        Assert.Contains("Verification token has already been used.", exception.Errors[nameof(VerifyEmailRequestDto.Token)]);
    }

    [Fact]
    public async Task VerifyEmailAsync_EmptyTokenThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        var authService = CreateAuthService(dbContext);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            authService.VerifyEmailAsync(new VerifyEmailRequestDto { Token = " " }));

        Assert.Contains("Token is required.", exception.Errors[nameof(VerifyEmailRequestDto.Token)]);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext)
    {
        return new AuthService(
            new AuthRepository(dbContext),
            new PasswordService(),
            new TokenService(Options.Create(TestJwtSettings)),
            NullLogger<AuthService>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedUnverifiedUserWithTokenAsync(
        AppDbContext dbContext,
        string token,
        DateTime expiresAt)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            PasswordHash = "hash",
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
