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
    public async Task GetById_ReturnsPropertyDetailsWithRequiredData()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyWithDetailsAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(dto);
        Assert.Equal(propertyId, dto!.Id);
        Assert.NotNull(dto.PropertyType);
        Assert.NotNull(dto.PropertyStatus);
        Assert.NotNull(dto.Company);
        Assert.NotNull(dto.Agent);
        Assert.NotNull(dto.Latitude);
        Assert.NotNull(dto.Longitude);
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

        public async Task<int> SeedPropertyWithDetailsAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var propertyType = await dbContext.PropertyTypes.FirstOrDefaultAsync()
                ?? dbContext.PropertyTypes.Add(new PropertyType
            {
                Name = "Apartment",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }).Entity;

            var propertyStatus = await dbContext.PropertyStatuses.FirstOrDefaultAsync()
                ?? dbContext.PropertyStatuses.Add(new PropertyStatus
            {
                Name = "Available",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }).Entity;

            var company = await dbContext.Companies
                .FirstOrDefaultAsync(x => x.Name == "Prime Real Estate")
                ?? dbContext.Companies.Add(new Company
            {
                Name = "Prime Real Estate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }).Entity;

            var agent = await dbContext.Agents
                .FirstOrDefaultAsync(x => x.Email == "ardit.hoxha@estateiq.local")
                ?? dbContext.Agents.Add(new Agent
            {
                FirstName = "Ardit",
                LastName = "Hoxha",
                Email = "ardit.hoxha@estateiq.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }).Entity;

            await dbContext.SaveChangesAsync();

            var relationshipExists = await dbContext.AgentCompanies.AnyAsync(x => x.AgentId == agent.Id && x.CompanyId == company.Id);

            if (!relationshipExists)
            {
                dbContext.AgentCompanies.Add(new AgentCompany
                {
                    AgentId = agent.Id,
                    CompanyId = company.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await dbContext.SaveChangesAsync();
            }

            var property = new Property
            {
                Title = "Downtown Apartment",
                Description = "Property details endpoint test",
                Price = 150000m,
                Area = 85m,
                Bedrooms = 2,
                Bathrooms = 1,
                Floors = 1,
                YearBuilt = 2020,
                PropertyTypeId = propertyType.Id,
                PropertyStatusId = propertyStatus.Id,
                CompanyId = company.Id,
                AgentId = agent.Id,
                Address = "Main Street 12",
                City = "Prishtine",
                Latitude = 42.6629m,
                Longitude = 21.1655m,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            return property.Id;
        }
    }
}
