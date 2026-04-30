using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.TryGetProperty("items", out _));
        Assert.True(document.RootElement.TryGetProperty("totalCount", out _));
        Assert.True(document.RootElement.TryGetProperty("page", out _));
        Assert.True(document.RootElement.TryGetProperty("pageSize", out _));
        Assert.True(document.RootElement.TryGetProperty("totalPages", out _));
        Assert.False(document.RootElement.TryGetProperty("pageNumber", out _));

        var result = JsonSerializer.Deserialize<PagedResult<PropertyDto>>(content, JsonSerializerOptions.Web);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);

        var property = result.Items.Single();
        Assert.Equal("Modern Apartment", property.Title);
        Assert.Equal("Apartment", property.PropertyType.Name);
        Assert.Equal(41.3275m, property.Latitude);
        Assert.Equal(19.8187m, property.Longitude);
    }

    [Fact]
    public async Task GetProperties_WithQueryParameters_ReturnsFilteredPage()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertiesAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties?city=Tirane&propertyTypeId=1&propertyStatusId=1&minPrice=100000&maxPrice=150000&search=renovated&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PropertyDto>>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Modern Apartment", result.Items.Single().Title);
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
        Assert.Equal(41.3275m, result.Latitude);
        Assert.Equal(19.8187m, result.Longitude);
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
        Assert.Equal(41.3275m, result.Latitude);
        Assert.Equal(19.8187m, result.Longitude);
    }

    [Fact]
    public async Task DeleteProperty_ExistingProperty_ReturnsNoContentAndDeletesProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/properties/{propertyId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProperty_MissingProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/properties/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProperty_UnderContractProperty_ReturnsConflict()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync(propertyStatusId: 6);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Sold, rented, or under-contract properties cannot be deleted.", await response.Content.ReadAsStringAsync());
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

        public async Task<int> SeedPropertyAsync(int propertyStatusId = 1)
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
                PropertyStatusId = propertyStatusId,
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

        public async Task SeedPropertiesAsync()
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);

            dbContext.Properties.AddRange(
                new Property
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
                },
                new Property
                {
                    Title = "Coastal Apartment",
                    Description = "Sea view apartment",
                    Price = 180000m,
                    Area = 92m,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Floors = 1,
                    YearBuilt = 2019,
                    PropertyTypeId = 1,
                    PropertyStatusId = 1,
                    CompanyId = 1,
                    AgentId = 1,
                    Address = "Rruga Taulantia",
                    City = "Durres",
                    Latitude = 41.3133m,
                    Longitude = 19.4469m,
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();
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

            if (!await dbContext.PropertyStatuses.AnyAsync(propertyStatus => propertyStatus.Id == 6))
            {
                dbContext.PropertyStatuses.Add(new PropertyStatus
                {
                    Id = 6,
                    Name = "Under Contract",
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
