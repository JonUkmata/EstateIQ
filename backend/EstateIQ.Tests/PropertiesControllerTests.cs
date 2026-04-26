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

public class PropertiesControllerTests
{
    [Fact]
    public async Task GetProperties_ReturnsSeededProperties()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Modern Apartment", result[0].Title);
        Assert.Equal("Apartment", result[0].PropertyType.Name);
    }

    [Fact]
    public async Task GetProperty_ExistingId_ReturnsProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal(propertyId, result!.Id);
        Assert.Equal("EstateIQ", result.Company.Name);
    }

    [Fact]
    public async Task GetProperty_MissingId_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_ValidPayload_ReturnsCreatedProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal("Modern Apartment", result!.Title);
        Assert.Equal("For Sale", result.PropertyStatus.Name);
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

        public async Task SeedReferenceDataAsync()
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);
        }

        public async Task<int> SeedPropertyAsync()
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);

            var property = new Property
            {
                Title = "Modern Apartment",
                Description = "Freshly renovated apartment",
                Price = 120000m,
                Area = 78m,
                Bedrooms = 2,
                Bathrooms = 1,
                Floors = 1,
                YearBuilt = 2021,
                PropertyTypeId = 1,
                PropertyStatusId = 1,
                CompanyId = 1,
                AgentId = 1,
                Address = "Rruga e Kavajes",
                City = "Tirane",
                Latitude = 41.3275m,
                Longitude = 19.8187m,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            return property.Id;
        }

        private static async Task SeedReferenceDataAsync(AppDbContext dbContext)
        {
            if (!await dbContext.PropertyTypes.AnyAsync(propertyType => propertyType.Id == 1))
            {
                dbContext.PropertyTypes.Add(new PropertyType
                {
                    Id = 1,
                    Name = "Apartment",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (!await dbContext.PropertyStatuses.AnyAsync(propertyStatus => propertyStatus.Id == 1))
            {
                dbContext.PropertyStatuses.Add(new PropertyStatus
                {
                    Id = 1,
                    Name = "For Sale",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            dbContext.Companies.Add(new Company
            {
                Id = 1,
                Name = "EstateIQ",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            dbContext.Agents.Add(new Agent
            {
                Id = 1,
                FirstName = "Valon",
                LastName = "Dobrunaj",
                Email = "valon@estateiq.local",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = 1,
                AgentId = 1,
                CompanyId = 1,
                Role = "Senior Agent",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static CreatePropertyDto BuildCreateDto()
    {
        return new CreatePropertyDto
        {
            Title = "Modern Apartment",
            Description = "Freshly renovated apartment",
            Price = 120000m,
            Area = 78m,
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2021,
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            CompanyId = 1,
            AgentId = 1,
            Address = "Rruga e Kavajes",
            City = "Tirane",
            Latitude = 41.3275m,
            Longitude = 19.8187m
        };
    }
}
