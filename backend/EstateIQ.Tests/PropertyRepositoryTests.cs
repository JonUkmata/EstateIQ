using EstateIQ.Data;
using EstateIQ.Models;
using EstateIQ.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class PropertyRepositoryTests
{
    [Fact]
    public async Task CreateAsync_SavesEntity()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var repository = new PropertyRepository(dbContext);

        var property = BuildProperty("Create Test", "Tirane", 150000m);

        var created = await repository.CreateAsync(property);

        Assert.True(created.Id > 0);
        Assert.Equal(1, await dbContext.Properties.CountAsync());
        Assert.NotEqual(default, created.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectEntity()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = BuildProperty("Lookup Property", "Tirane", 180000m);
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByIdAsync(property.Id);

        Assert.NotNull(result);
        Assert.Equal("Lookup Property", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        await using var dbContext = CreateContext();
        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangesAndSetsUpdatedAt()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = BuildProperty("Old Title", "Tirane", 200000m);
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        property.Title = "Updated Title";
        property.Price = 225000m;

        var updated = await repository.UpdateAsync(property);

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal(225000m, updated.Price);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = BuildProperty("Delete Me", "Durres", 90000m);
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var deleted = await repository.DeleteAsync(property.Id);

        Assert.True(deleted);
        Assert.False(await repository.ExistsAsync(property.Id));
    }

    [Fact]
    public async Task GetByCityAsync_FiltersResults()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        dbContext.Properties.AddRange(
            BuildProperty("Tirane One", "Tirane", 120000m),
            BuildProperty("Tirane Two", "Tirane", 130000m),
            BuildProperty("Durres One", "Durres", 110000m));
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var results = (await repository.GetByCityAsync("Tirane")).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, x => Assert.Equal("Tirane", x.City));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsExpectedPageAndSortOrder()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        dbContext.Properties.AddRange(
            BuildProperty("Alpha", "Tirane", 300000m),
            BuildProperty("Beta", "Tirane", 100000m),
            BuildProperty("Gamma", "Tirane", 200000m));
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var (items, totalCount) = await repository.GetPagedAsync(1, 2, "price", ascending: true);
        var itemList = items.ToList();

        Assert.Equal(3, totalCount);
        Assert.Equal(2, itemList.Count);
        Assert.Equal(["Beta", "Gamma"], itemList.Select(x => x.Title).ToArray());
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_LoadsNavigationProperties()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = BuildProperty("Detailed Property", "Vlore", 210000m);
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByIdWithDetailsAsync(property.Id);

        Assert.NotNull(result);
        Assert.NotNull(result!.Agent);
        Assert.NotNull(result.Company);
        Assert.NotNull(result.PropertyType);
        Assert.NotNull(result.PropertyStatus);
    }

    [Fact]
    public async Task SearchAsync_SearchesTitleAndDescription()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        dbContext.Properties.AddRange(
            BuildProperty("Sea View Apartment", "Durres", 190000m, "Beautiful coast apartment"),
            BuildProperty("City Office", "Tirane", 300000m, "Prime business district"),
            BuildProperty("Family House", "Shkoder", 140000m, "Quiet neighborhood"));
        await dbContext.SaveChangesAsync();

        var repository = new PropertyRepository(dbContext);

        var results = (await repository.SearchAsync("business")).ToList();

        Assert.Single(results);
        Assert.Equal("City Office", results[0].Title);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedReferenceDataAsync(AppDbContext dbContext)
    {
        dbContext.PropertyTypes.Add(new PropertyType
        {
            Id = 1,
            Name = "Apartment",
            CreatedAt = DateTime.Now,
            IsActive = true
        });

        dbContext.PropertyStatuses.Add(new PropertyStatus
        {
            Id = 1,
            Name = "For Sale",
            ColorCode = "#007bff",
            CreatedAt = DateTime.Now,
            IsActive = true
        });

        dbContext.Companies.Add(new Company
        {
            Id = 1,
            Name = "EstateIQ",
            CreatedAt = DateTime.Now,
            IsActive = true
        });

        dbContext.Agents.Add(new Agent
        {
            Id = 1,
            FirstName = "Valon",
            LastName = "Demo",
            Email = "valon.demo@estateiq.local",
            CreatedAt = DateTime.Now,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }

    private static Property BuildProperty(string title, string city, decimal price, string? description = null)
    {
        return new Property
        {
            Title = title,
            Description = description ?? $"{title} description",
            Price = price,
            Area = 100m,
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2020,
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            CompanyId = 1,
            AgentId = 1,
            Address = "Test Address",
            City = city,
            Latitude = 41.3275m,
            Longitude = 19.8187m
        };
    }
}
