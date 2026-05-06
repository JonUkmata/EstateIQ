using System.Net;
using System.Net.Http.Json;
using EstateIQ.Data;
using EstateIQ.DTOs.Auth;
using EstateIQ.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EstateIQ.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedAndPersistsRegistration()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "Jon@Example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Registration successful. Please verify your email before logging in.", result!.Message);
        Assert.False(string.IsNullOrWhiteSpace(result.VerificationToken));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var userRole = await dbContext.UserRoles.SingleAsync();
        var role = await dbContext.Roles.SingleAsync(x => x.Id == userRole.RoleId);
        var verificationToken = await dbContext.EmailVerificationTokens.SingleAsync();

        Assert.Equal("jon@example.com", user.Email);
        Assert.False(user.IsEmailConfirmed);
        Assert.True(user.IsActive);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Equal("User", role.Name);
        Assert.Equal(result.VerificationToken, verificationToken.Token);
    }

    [Fact]
    public async Task VerifyEmail_ValidToken_ReturnsOkAndConfirmsUser()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseDto>();

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequestDto
        {
            Token = registerResult!.VerificationToken
        });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<VerifyEmailResponseDto>();
        Assert.NotNull(verifyResult);
        Assert.Equal("Email verified successfully. You can now login.", verifyResult!.Message);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var verificationToken = await dbContext.EmailVerificationTokens.SingleAsync();

        Assert.True(user.IsEmailConfirmed);
        Assert.NotNull(verificationToken.UsedAt);
    }

    [Fact]
    public async Task Login_UnverifiedUser_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "jon@example.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
        var content = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains("Email is not verified.", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_VerifiedUser_ReturnsTokensAndPersistsHashedRefreshToken()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseDto>();
        await client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequestDto
        {
            Token = registerResult!.VerificationToken
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "jon@example.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrWhiteSpace(loginResult!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginResult.RefreshToken));
        Assert.Equal("jon@example.com", loginResult.User.Email);
        Assert.Equal(["User"], loginResult.User.Roles);
        Assert.Equal(["BookViewing", "ViewProperties"], loginResult.User.Permissions.Order().ToArray());
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, cookie => cookie.StartsWith("refreshToken=", StringComparison.Ordinal));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotEqual(loginResult.RefreshToken, refreshToken.TokenHash);
    }

    [Fact]
    public async Task Refresh_ValidBodyRefreshToken_ReturnsNewAccessToken()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var loginResult = await RegisterVerifyAndLoginAsync(client);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto
        {
            RefreshToken = loginResult.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
        Assert.NotNull(refreshResult);
        Assert.False(string.IsNullOrWhiteSpace(refreshResult!.AccessToken));
        Assert.True(refreshResult.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Refresh_MissingRefreshToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto());

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_ValidBodyRefreshToken_RevokesTokenAndRefreshFails()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var loginResult = await RegisterVerifyAndLoginAsync(client);

        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequestDto
        {
            RefreshToken = loginResult.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        var logoutResult = await logoutResponse.Content.ReadFromJsonAsync<LogoutResponseDto>();
        Assert.NotNull(logoutResult);
        Assert.Equal("Logged out successfully.", logoutResult!.Message);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedRefreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotNull(storedRefreshToken.RevokedAt);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto
        {
            RefreshToken = loginResult.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_MissingRefreshToken_ReturnsSuccess()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequestDto());

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        var logoutResult = await logoutResponse.Content.ReadFromJsonAsync<LogoutResponseDto>();
        Assert.NotNull(logoutResult);
        Assert.Equal("Logged out successfully.", logoutResult!.Message);
    }

    private static async Task<LoginResponseDto> RegisterVerifyAndLoginAsync(HttpClient client)
    {
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            FirstName = "Jon",
            LastName = "Ukmata",
            Email = "jon@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseDto>();
        await client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequestDto
        {
            Token = registerResult!.VerificationToken
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "jon@example.com",
            Password = "Password123!"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginResult!;
    }

    private sealed class EstateIqWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EstateIQTests;Trusted_Connection=True;TrustServerCertificate=True",
                    ["Redis:ConnectionString"] = "localhost:6379",
                    ["Jwt:Issuer"] = "EstateIQ.Tests",
                    ["Jwt:Audience"] = "EstateIQ.Tests",
                    ["Jwt:Key"] = "EstateIQ-Tests-Jwt-Key-Minimum-32-Bytes-2026",
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:RefreshTokenDays"] = "7"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
