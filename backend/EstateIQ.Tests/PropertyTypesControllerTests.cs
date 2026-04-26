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

public class PropertyTypesControllerTests
{
    [Fact]
    public async Task GetPropertyTypes_DefaultRequest_ReturnsRequiredSeededTypes()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/propertytypes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PropertyTypeDto>>();
        Assert.NotNull(result);
        Assert.True(result!.Count >= 5);

        var typeNames = result.Select(propertyType => propertyType.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("House", typeNames);
        Assert.Contains("Apartment", typeNames);
        Assert.Contains("Villa", typeNames);
        Assert.Contains("Land", typeNames);
        Assert.Contains("Commercial", typeNames);
    }

    [Fact]
    public async Task GetPropertyTypes_Search_ReturnsFilteredResults()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertyTypesAsync(
            new PropertyType { Id = 101, Name = "House", IsActive = true, CreatedAt = DateTime.UtcNow },
            new PropertyType { Id = 102, Name = "Apartment", IsActive = true, CreatedAt = DateTime.UtcNow },
            new PropertyType { Id = 103, Name = "Villa", IsActive = false, CreatedAt = DateTime.UtcNow });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/propertytypes?search=Apart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PropertyTypeDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!);
        Assert.All(result, propertyType => Assert.Contains("Apart", propertyType.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result, propertyType => propertyType.Name == "Apartment");
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

        public async Task SeedPropertyTypesAsync(params PropertyType[] propertyTypes)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.PropertyTypes.AddRange(propertyTypes);
            await dbContext.SaveChangesAsync();
        }
    }
}
