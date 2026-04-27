using AutoMapper;
using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Mappings;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EstateIQ.Tests;

public class PropertyServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidData_ReturnsPropertyDto()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var service = CreateService(dbContext);

        var dto = BuildCreateDto();

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal("Apartment", result.PropertyType.Name);
        Assert.Equal("EstateIQ", result.Company.Name);
    }

    [Fact]
    public async Task CreateAsync_InvalidPropertyType_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var service = CreateService(dbContext);
        var dto = BuildCreateDto();
        dto.PropertyTypeId = 999;

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));

        Assert.Contains(nameof(CreatePropertyDto.PropertyTypeId), exception.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_AgentWithoutCompanyRelationship_ThrowsBusinessRuleException()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext, includeAgentCompany: false);
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(BuildCreateDto()));

        Assert.Equal("Agent is not associated with this company.", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsPropertyDto()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.GetByIdAsync(property.Id);

        Assert.NotNull(result);
        Assert.Equal(property.Title, result!.Title);
        Assert.Equal("For Sale", result.PropertyStatus.Name);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ThrowsNotFoundException()
    {
        await using var dbContext = CreateContext();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
    }

    [Fact]
    public async Task UpdateAsync_SoldPropertyPriceChange_ThrowsBusinessRuleException()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, propertyStatusId: 2);
        var service = CreateService(dbContext);
        var dto = BuildUpdateDto(property);
        dto.Price = 999999m;

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateAsync(property.Id, dto));

        Assert.Equal("Sold properties cannot have their price changed.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RentedPropertyReadOnlyFieldChange_ThrowsBusinessRuleException()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, propertyStatusId: 3);
        var service = CreateService(dbContext);
        var dto = BuildUpdateDto(property);
        dto.City = "Durres";

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateAsync(property.Id, dto));

        Assert.Equal("Rented properties have read-only structural and assignment fields.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_SoldProperty_ThrowsBusinessRuleException()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, propertyStatusId: 2);
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(property.Id));
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectResults()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        await SeedPropertyAsync(dbContext, title: "Sea View Apartment", description: "Beautiful apartment");
        await SeedPropertyAsync(dbContext, title: "Office Space", description: "Business center office");
        var service = CreateService(dbContext);

        var results = (await service.SearchAsync("Business")).ToList();

        Assert.Single(results);
        Assert.Equal("Office Space", results[0].Title);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPageData()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        await SeedPropertyAsync(dbContext, title: "Property 1", price: 300000m);
        await SeedPropertyAsync(dbContext, title: "Property 2", price: 100000m);
        await SeedPropertyAsync(dbContext, title: "Property 3", price: 200000m);
        var service = CreateService(dbContext);

        var result = await service.GetPagedAsync(1, 2, "price", true);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(["Property 2", "Property 3"], result.Items.Select(x => x.Title).ToArray());
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsFilteredPageData()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        await SeedPropertyAsync(dbContext, title: "Modern Apartment", description: "Freshly renovated apartment", price: 120000m);
        await SeedPropertyAsync(dbContext, title: "Luxury Villa", description: "Renovated villa", price: 450000m);
        await SeedPropertyAsync(dbContext, title: "Office Space", description: "Business center", price: 140000m);
        var service = CreateService(dbContext);

        var result = await service.GetFilteredAsync(new PropertyQueryParameters
        {
            City = "Tirane",
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            MinPrice = 100000m,
            MaxPrice = 150000m,
            Search = "renovated",
            Page = 1,
            PageSize = 10
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Single(result.Items);
        Assert.Equal("Modern Apartment", result.Items.Single().Title);
    }

    [Fact]
    public async Task UpdatePriceAsync_UpdatesPriceAndTimestamp()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, price: 150000m);
        var service = CreateService(dbContext);

        var result = await service.UpdatePriceAsync(property.Id, 175000m);

        Assert.Equal(175000m, result.Price);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_UpdatesPropertyStatus()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, propertyStatusId: 1);
        var service = CreateService(dbContext);

        var result = await service.ChangeStatusAsync(property.Id, 2);

        Assert.Equal("Sold", result.PropertyStatus.Name);
    }

    [Fact]
    public async Task CanDeleteAsync_RentedProperty_ReturnsFalse()
    {
        await using var dbContext = CreateContext();
        await SeedReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, propertyStatusId: 3);
        var service = CreateService(dbContext);

        var result = await service.CanDeleteAsync(property.Id);

        Assert.False(result);
    }

    [Fact]
    public void MappingProfile_IsValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        configuration.AssertConfigurationIsValid();
    }

    private static PropertyService CreateService(AppDbContext dbContext)
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        return new PropertyService(
            new PropertyRepository(dbContext),
            new PropertyTypeRepository(dbContext),
            new PropertyStatusRepository(dbContext),
            new CompanyRepository(dbContext),
            new AgentRepository(dbContext),
            new AgentCompanyRepository(dbContext),
            mapper,
            NullLogger<PropertyService>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedReferenceDataAsync(AppDbContext dbContext, bool includeAgentCompany = true)
    {
        dbContext.PropertyTypes.Add(new PropertyType
        {
            Id = 1,
            Name = "Apartment",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        dbContext.PropertyStatuses.AddRange(
            new PropertyStatus
            {
                Id = 1,
                Name = "For Sale",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new PropertyStatus
            {
                Id = 2,
                Name = "Sold",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new PropertyStatus
            {
                Id = 3,
                Name = "Rented",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

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

        if (includeAgentCompany)
        {
            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = 1,
                AgentId = 1,
                CompanyId = 1,
                Role = "Senior Agent",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Property> SeedPropertyAsync(
        AppDbContext dbContext,
        int propertyStatusId = 1,
        string title = "Test Property",
        string description = "Default description",
        decimal price = 100000m)
    {
        var property = new Property
        {
            Title = title,
            Description = description,
            Price = price,
            Area = 85m,
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2020,
            PropertyTypeId = 1,
            PropertyStatusId = propertyStatusId,
            CompanyId = 1,
            AgentId = 1,
            Address = "Main Street",
            City = "Tirane",
            Latitude = 41.3275m,
            Longitude = 19.8187m,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        return property;
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

    private static UpdatePropertyDto BuildUpdateDto(Property property)
    {
        return new UpdatePropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Area = property.Area,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Floors = property.Floors,
            YearBuilt = property.YearBuilt,
            PropertyTypeId = property.PropertyTypeId,
            PropertyStatusId = property.PropertyStatusId,
            CompanyId = property.CompanyId,
            AgentId = property.AgentId,
            Address = property.Address,
            City = property.City,
            Latitude = property.Latitude,
            Longitude = property.Longitude
        };
    }
}
