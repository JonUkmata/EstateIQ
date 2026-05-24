using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs.Dashboard;
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

public class DashboardControllerTests
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
    public async Task GetMyDashboard_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyDashboard_AdminRole_ReturnsAdminDashboard()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 2, soldCount: 1);
        using var client = factory.CreateClient();
        AddBearerToken(client, Guid.NewGuid(), [Roles.Admin]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Admin", json.GetProperty("role").GetString());
        Assert.Equal(3, json.GetProperty("totalProperties").GetInt32());
        Assert.Equal(2, json.GetProperty("forSaleProperties").GetInt32());
        Assert.Equal(1, json.GetProperty("soldProperties").GetInt32());
        Assert.Equal(1, json.GetProperty("totalCompanies").GetInt32());
        Assert.Equal(1, json.GetProperty("totalAgents").GetInt32());
        Assert.Equal(3, json.GetProperty("recentProperties").GetArrayLength());
    }

    [Fact]
    public async Task GetMyDashboard_AdminRole_TotalUsersCountIsCorrect()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 1, soldCount: 0);
        await factory.SeedExtraUsersAsync(count: 3);
        using var client = factory.CreateClient();
        AddBearerToken(client, Guid.NewGuid(), [Roles.Admin]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        // 1 agent user seeded by SeedCompanyWithAgentAsync + 3 extra = 4
        Assert.Equal(4, json.GetProperty("totalUsers").GetInt32());
    }

    [Fact]
    public async Task GetMyDashboard_CompanyAdminRole_ReturnsCompanyScopedDashboard()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        var companyAdminUserId = Guid.NewGuid();
        await factory.SeedCompanyAdminUserAsync(companyAdminUserId, companyId);
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 2, soldCount: 1);
        using var client = factory.CreateClient();
        AddBearerToken(client, companyAdminUserId, [Roles.CompanyAdmin]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CompanyAdmin", json.GetProperty("role").GetString());
        Assert.Equal(companyId, json.GetProperty("companyId").GetInt32());
        Assert.Equal(3, json.GetProperty("companyProperties").GetInt32());
        Assert.Equal(2, json.GetProperty("forSaleProperties").GetInt32());
        Assert.Equal(1, json.GetProperty("soldProperties").GetInt32());
        Assert.Equal(1, json.GetProperty("companyAgents").GetInt32());
    }

    [Fact]
    public async Task GetMyDashboard_CompanyAdminRole_DoesNotSeeOtherCompanyProperties()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        var companyAdminUserId = Guid.NewGuid();
        await factory.SeedCompanyAdminUserAsync(companyAdminUserId, companyId);
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 2, soldCount: 0);

        // Seed a second company with its own properties
        var (_, company2Id, agent2Id) = await factory.SeedSecondCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agent2Id, company2Id, forSaleCount: 3, soldCount: 0);

        using var client = factory.CreateClient();
        AddBearerToken(client, companyAdminUserId, [Roles.CompanyAdmin]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, json.GetProperty("companyProperties").GetInt32());
    }

    [Fact]
    public async Task GetMyDashboard_AgentRole_ReturnsAgentScopedDashboard()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (agentUserId, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 1, soldCount: 2);
        using var client = factory.CreateClient();
        AddBearerToken(client, agentUserId, [Roles.Agent]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Agent", json.GetProperty("role").GetString());
        Assert.Equal(agentId, json.GetProperty("agentId").GetInt32());
        Assert.Equal(3, json.GetProperty("myProperties").GetInt32());
        Assert.Equal(1, json.GetProperty("myForSaleProperties").GetInt32());
        Assert.Equal(2, json.GetProperty("mySoldProperties").GetInt32());
    }

    [Fact]
    public async Task GetMyDashboard_AgentRole_DoesNotSeeOtherAgentProperties()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (agentUserId, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 2, soldCount: 0);

        // Seed a second agent with their own properties under the same company
        var (_, agent2Id) = await factory.SeedSecondAgentAsync(companyId);
        await factory.SeedPropertiesAsync(agent2Id, companyId, forSaleCount: 5, soldCount: 0);

        using var client = factory.CreateClient();
        AddBearerToken(client, agentUserId, [Roles.Agent]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, json.GetProperty("myProperties").GetInt32());
    }

    [Fact]
    public async Task GetMyDashboard_UserRole_ReturnsUserDashboard()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 3, soldCount: 1);
        using var client = factory.CreateClient();
        AddBearerToken(client, Guid.NewGuid(), [Roles.User]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("User", json.GetProperty("role").GetString());
        Assert.Equal(3, json.GetProperty("availableProperties").GetInt32());
        Assert.True(json.GetProperty("latestProperties").GetArrayLength() > 0);
        Assert.True(json.GetProperty("popularCities").GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetMyDashboard_AdminPrecedesCompanyAdmin_ReturnsAdminDashboard()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var (_, companyId, agentId) = await factory.SeedCompanyWithAgentAsync();
        var companyAdminUserId = Guid.NewGuid();
        await factory.SeedCompanyAdminUserAsync(companyAdminUserId, companyId);
        await factory.SeedPropertiesAsync(agentId, companyId, forSaleCount: 1, soldCount: 0);
        using var client = factory.CreateClient();

        // Token has both Admin and CompanyAdmin roles — Admin should take precedence
        AddBearerToken(client, companyAdminUserId, [Roles.Admin, Roles.CompanyAdmin]);

        var response = await client.GetAsync("/api/dashboard/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Admin", json.GetProperty("role").GetString());
        Assert.True(json.TryGetProperty("totalProperties", out _));
    }

    private static void AddBearerToken(HttpClient client, Guid userId, string[] roles)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(userId, roles));
    }

    private static string GenerateToken(Guid userId, string[] roles)
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));

        return tokenService.GenerateAccessToken(
            new User { Id = userId, Email = "dashboard-test@example.com" },
            roles,
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

        // Seeds one company + one agent linked to a new user. Returns (agentUserId, companyId, agentId).
        public async Task<(Guid AgentUserId, int CompanyId, int AgentId)> SeedCompanyWithAgentAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var agentUserId = Guid.NewGuid();

            dbContext.Companies.Add(new Company
            {
                Id = 1,
                Name = "Test Company",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.Users.Add(new User
            {
                Id = agentUserId,
                FirstName = "Agent",
                LastName = "One",
                Email = "agent1@example.com",
                PasswordHash = "hash",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.Agents.Add(new Agent
            {
                Id = 1,
                UserId = agentUserId,
                FirstName = "Agent",
                LastName = "One",
                Email = "agent1@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = 1,
                AgentId = 1,
                CompanyId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return (agentUserId, 1, 1);
        }

        // Seeds a second company + agent. Must be called after SeedCompanyWithAgentAsync.
        public async Task<(Guid AgentUserId, int CompanyId, int AgentId)> SeedSecondCompanyWithAgentAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var agentUserId = Guid.NewGuid();

            dbContext.Companies.Add(new Company
            {
                Id = 2,
                Name = "Other Company",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.Users.Add(new User
            {
                Id = agentUserId,
                FirstName = "Agent",
                LastName = "Two",
                Email = "agent2@example.com",
                PasswordHash = "hash",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.Agents.Add(new Agent
            {
                Id = 2,
                UserId = agentUserId,
                FirstName = "Agent",
                LastName = "Two",
                Email = "agent2@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = 2,
                AgentId = 2,
                CompanyId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return (agentUserId, 2, 2);
        }

        // Seeds a second agent under an existing company. Returns (agentUserId, agentId).
        public async Task<(Guid AgentUserId, int AgentId)> SeedSecondAgentAsync(int companyId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var agentUserId = Guid.NewGuid();
            var nextAgentId = await dbContext.Agents.MaxAsync(a => (int?)a.Id) + 1 ?? 2;
            var nextAgentCompanyId = await dbContext.AgentCompanies.MaxAsync(ac => (int?)ac.Id) + 1 ?? 2;

            dbContext.Users.Add(new User
            {
                Id = agentUserId,
                FirstName = "Agent",
                LastName = "Extra",
                Email = $"agent-extra-{nextAgentId}@example.com",
                PasswordHash = "hash",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.Agents.Add(new Agent
            {
                Id = nextAgentId,
                UserId = agentUserId,
                FirstName = "Agent",
                LastName = "Extra",
                Email = $"agent-extra-{nextAgentId}@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = nextAgentCompanyId,
                AgentId = nextAgentId,
                CompanyId = companyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return (agentUserId, nextAgentId);
        }

        // Creates a CompanyAdmin user record linked to the given company. Must be called after SeedCompanyWithAgentAsync.
        public async Task SeedCompanyAdminUserAsync(Guid userId, int companyId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FirstName = "Company",
                LastName = "Admin",
                Email = "companyadmin@example.com",
                PasswordHash = "hash",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            });

            dbContext.CompanyUsers.Add(new CompanyUser
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = companyId,
                RelationshipType = Roles.CompanyAdmin,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        // Seeds additional plain users (no roles/relations).
        public async Task SeedExtraUsersAsync(int count)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            for (var i = 0; i < count; i++)
            {
                dbContext.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "User",
                    LastName = $"{i + 1}",
                    Email = $"user{i + 1}@example.com",
                    PasswordHash = "hash",
                    IsActive = true,
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();
        }

        // Seeds properties for a given agent and company. Uses status IDs from seeded data.
        // For Sale = status 1, Sold = status 3.
        public async Task SeedPropertiesAsync(int agentId, int companyId, int forSaleCount, int soldCount)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var nextId = await dbContext.Properties.MaxAsync(p => (int?)p.Id) + 1 ?? 1;

            for (var i = 0; i < forSaleCount; i++, nextId++)
            {
                dbContext.Properties.Add(new Property
                {
                    Id = nextId,
                    Title = $"Property {nextId}",
                    Price = 100_000m + nextId * 1000,
                    Area = 80m,
                    PropertyTypeId = 1,
                    PropertyStatusId = 1, // For Sale
                    CompanyId = companyId,
                    AgentId = agentId,
                    Address = $"Street {nextId}",
                    City = "Prishtina",
                    CreatedAt = DateTime.UtcNow.AddDays(-nextId)
                });
            }

            for (var i = 0; i < soldCount; i++, nextId++)
            {
                dbContext.Properties.Add(new Property
                {
                    Id = nextId,
                    Title = $"Property {nextId}",
                    Price = 150_000m + nextId * 1000,
                    Area = 90m,
                    PropertyTypeId = 1,
                    PropertyStatusId = 3, // Sold
                    CompanyId = companyId,
                    AgentId = agentId,
                    Address = $"Street {nextId}",
                    City = "Prizren",
                    CreatedAt = DateTime.UtcNow.AddDays(-nextId)
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
