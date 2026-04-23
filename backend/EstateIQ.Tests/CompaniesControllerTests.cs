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

public class CompaniesControllerTests
{
    [Fact]
    public async Task GetCompanies_DefaultRequest_ReturnsOnlyActiveCompanies()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompaniesAsync(
            new Company { Id = 1, Name = "ABC Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "Inactive Co", City = "Gjakove", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "XYZ Properties", City = "Peje", IsActive = true, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CompanyDropdownDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal(["ABC Real Estate", "XYZ Properties"], result.Select(company => company.Name).ToArray());
        Assert.All(result, company => Assert.True(company.IsActive));
    }

    [Fact]
    public async Task GetCompanies_IncludeInactiveTrue_ReturnsAllCompanies()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompaniesAsync(
            new Company { Id = 1, Name = "ABC Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "Inactive Co", City = "Gjakove", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "XYZ Properties", City = "Peje", IsActive = true, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies?includeInactive=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CompanyDropdownDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.Count);
        Assert.Contains(result, company => company.Name == "Inactive Co" && !company.IsActive);
    }

    [Fact]
    public async Task GetCompanies_Search_ReturnsFilteredResults()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompaniesAsync(
            new Company { Id = 1, Name = "ABC Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "ABC Legacy", City = "Gjakove", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "XYZ Properties", City = "Peje", IsActive = true, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies?search=ABC");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CompanyDropdownDto>>();
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("ABC Real Estate", result[0].Name);
    }

    [Fact]
    public async Task GetCompanies_EmptyDatabase_ReturnsEmptyArray()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CompanyDropdownDto>>();
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

        public async Task SeedCompaniesAsync(params Company[] companies)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.Companies.AddRange(companies);
            await dbContext.SaveChangesAsync();
        }
    }
}
