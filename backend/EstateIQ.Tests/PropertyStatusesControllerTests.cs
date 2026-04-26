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

public class PropertyStatusesControllerTests
{
    [Fact]
    public async Task GetPropertyStatuses_DefaultRequest_ReturnsRequiredStatuses()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/propertystatuses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PropertyStatusDto>>();
        Assert.NotNull(result);

        var statusNames = result!.Select(status => status.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Available", statusNames);
        Assert.Contains("Pending", statusNames);
        Assert.Contains("Sold", statusNames);
    }

    [Fact]
    public async Task GetPropertyStatuses_RepeatedCalls_DoNotCreateDuplicates()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        _ = await client.GetAsync("/api/propertystatuses");
        _ = await client.GetAsync("/api/propertystatuses");

        var counts = await factory.GetStatusCountsAsync();
        Assert.Equal(1, counts["Available"]);
        Assert.Equal(1, counts["Pending"]);
        Assert.Equal(1, counts["Sold"]);
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
            await PropertyStatusSeeder.SeedRequiredPropertyStatusesAsync(dbContext);
        }

        public async Task<Dictionary<string, int>> GetStatusCountsAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await dbContext.PropertyStatuses
                .GroupBy(status => status.Name)
                .Select(group => new { Name = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count, StringComparer.OrdinalIgnoreCase);
        }
    }
}
