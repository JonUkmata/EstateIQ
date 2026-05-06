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
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Equal("User", role.Name);
        Assert.Equal(result.VerificationToken, verificationToken.Token);
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
