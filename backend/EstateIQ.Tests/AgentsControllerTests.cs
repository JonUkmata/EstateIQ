using System.Net;
using System.Net.Http.Json;
using EstateIQ.Data;
using EstateIQ.DTOs;
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

public class AgentsControllerTests
{
    [Fact]
    public async Task GetAgents_DefaultRequest_ReturnsOnlyActiveAgents()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedAgentsAsync(
            new Agent { Id = 1, FirstName = "Ardit", LastName = "Hoxha", Email = "ardit@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agent { Id = 2, FirstName = "Inactive", LastName = "Agent", Email = "inactive@estateiq.local", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Agent { Id = 3, FirstName = "Lea", LastName = "Krasniqi", Email = "lea@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/agents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<AgentDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal(["Ardit", "Lea"], result.Select(agent => agent.FirstName).ToArray());
        Assert.All(result, agent => Assert.True(agent.IsActive));
    }

    [Fact]
    public async Task GetAgents_IncludeInactiveTrue_ReturnsAllAgents()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedAgentsAsync(
            new Agent { Id = 1, FirstName = "Ardit", LastName = "Hoxha", Email = "ardit@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agent { Id = 2, FirstName = "Inactive", LastName = "Agent", Email = "inactive@estateiq.local", IsActive = false, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/agents?includeInactive=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<AgentDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Contains(result, agent => agent.FirstName == "Inactive" && !agent.IsActive);
    }

    [Fact]
    public async Task GetAgents_Search_ReturnsFilteredResults()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedAgentsAsync(
            new Agent { Id = 1, FirstName = "Ardit", LastName = "Hoxha", Email = "ardit@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agent { Id = 2, FirstName = "Lea", LastName = "Krasniqi", Email = "lea@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agent { Id = 3, FirstName = "Mira", LastName = "Berisha", Email = "mira@estateiq.local", IsActive = false, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/agents?search=Kras");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<AgentDto>>();
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Lea", result[0].FirstName);
    }

    [Fact]
    public async Task GetAgents_CompanyId_ReturnsAssignedActiveAgents()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedAgentsWithCompaniesAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/agents?companyId=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<AgentDto>>();
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Lea", result[0].FirstName);
    }

    [Fact]
    public async Task GetAgents_EmptyDatabase_ReturnsEmptyArray()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/agents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<AgentDto>>();
        Assert.NotNull(result);
        Assert.Empty(result!);
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
                    ["Redis:ConnectionString"] = "localhost:6379"
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

        public async Task SeedAgentsAsync(params Agent[] agents)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.Agents.AddRange(agents);
            await dbContext.SaveChangesAsync();
        }

        public async Task SeedAgentsWithCompaniesAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            dbContext.Companies.AddRange(
                new Company { Id = 1, Name = "Prime Real Estate", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Company { Id = 2, Name = "City Properties", IsActive = true, CreatedAt = DateTime.UtcNow });

            dbContext.Agents.AddRange(
                new Agent { Id = 1, FirstName = "Ardit", LastName = "Hoxha", Email = "ardit@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent { Id = 2, FirstName = "Lea", LastName = "Krasniqi", Email = "lea@estateiq.local", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent { Id = 3, FirstName = "Inactive", LastName = "Agent", Email = "inactive@estateiq.local", IsActive = false, CreatedAt = DateTime.UtcNow });

            dbContext.AgentCompanies.AddRange(
                new AgentCompany { Id = 1, AgentId = 1, CompanyId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new AgentCompany { Id = 2, AgentId = 2, CompanyId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new AgentCompany { Id = 3, AgentId = 3, CompanyId = 2, IsActive = true, CreatedAt = DateTime.UtcNow });

            await dbContext.SaveChangesAsync();
        }
    }
}
