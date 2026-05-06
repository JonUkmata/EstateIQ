using System.Net;
using System.Net.Http.Headers;
using EstateIQ.Data;
using EstateIQ.Models;
using EstateIQ.Services.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class JwtAuthenticationMiddlewareTests
{
    private static readonly JwtSettings TestJwtSettings = new()
    {
        Issuer = "EstateIQ",
        Audience = "EstateIQ",
        Key = "EstateIQ-Development-Jwt-Key-Replace-In-Production-2026",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    [Fact]
    public async Task PublicTestEndpoint_WorksWithoutToken()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedTestEndpoint_ReturnsUnauthorizedWithoutToken()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/test/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedTestEndpoint_SucceedsWithValidToken()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken());

        var response = await client.GetAsync("/api/test/protected");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string GenerateToken()
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));

        return tokenService.GenerateAccessToken(
            new User
            {
                Id = Guid.NewGuid(),
                Email = "jwt-test@example.com"
            },
            [],
            []);
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
                    ["Jwt:Issuer"] = TestJwtSettings.Issuer,
                    ["Jwt:Audience"] = TestJwtSettings.Audience,
                    ["Jwt:Key"] = TestJwtSettings.Key,
                    ["Jwt:AccessTokenMinutes"] = TestJwtSettings.AccessTokenMinutes.ToString(),
                    ["Jwt:RefreshTokenDays"] = TestJwtSettings.RefreshTokenDays.ToString()
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
