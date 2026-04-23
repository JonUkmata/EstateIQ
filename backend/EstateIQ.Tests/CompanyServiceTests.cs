using AutoMapper;
using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.Mappings;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EstateIQ.Tests;

public class CompanyServiceTests
{
    [Fact]
    public async Task GetForDropdownAsync_DefaultFilter_ReturnsOnlyActiveCompaniesSortedByName()
    {
        await using var dbContext = CreateContext();
        await SeedCompaniesAsync(
            dbContext,
            new Company { Id = 1, Name = "Zenith Properties", City = "Peje", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "Alpha Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "Beta Inactive", City = "Gjakove", IsActive = false, CreatedAt = DateTime.UtcNow });
        var service = CreateService(dbContext);

        var result = (await service.GetForDropdownAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, company => Assert.True(company.IsActive));
        Assert.Equal(["Alpha Real Estate", "Zenith Properties"], result.Select(company => company.Name).ToArray());
    }

    [Fact]
    public async Task GetForDropdownAsync_IncludeInactiveTrue_ReturnsAllCompaniesSortedByName()
    {
        await using var dbContext = CreateContext();
        await SeedCompaniesAsync(
            dbContext,
            new Company { Id = 1, Name = "Zenith Properties", City = "Peje", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "Alpha Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "Beta Inactive", City = "Gjakove", IsActive = false, CreatedAt = DateTime.UtcNow });
        var service = CreateService(dbContext);

        var result = (await service.GetForDropdownAsync(includeInactive: true)).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(
            ["Alpha Real Estate", "Beta Inactive", "Zenith Properties"],
            result.Select(company => company.Name).ToArray());
    }

    [Fact]
    public async Task GetForDropdownAsync_Search_ReturnsMatchingActiveCompaniesOnlyByDefault()
    {
        await using var dbContext = CreateContext();
        await SeedCompaniesAsync(
            dbContext,
            new Company { Id = 1, Name = "ABC Real Estate", City = "Prishtine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Id = 2, Name = "ABC Legacy", City = "Peje", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Company { Id = 3, Name = "XYZ Properties", City = "Gjakove", IsActive = true, CreatedAt = DateTime.UtcNow });
        var service = CreateService(dbContext);

        var result = (await service.GetForDropdownAsync(search: "ABC")).ToList();

        Assert.Single(result);
        Assert.Equal("ABC Real Estate", result[0].Name);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetForDropdownAsync_EmptyDatabase_ReturnsEmptyCollection()
    {
        await using var dbContext = CreateContext();
        var service = CreateService(dbContext);

        var result = await service.GetForDropdownAsync();

        Assert.Empty(result);
    }

    [Fact]
    public void MappingProfile_MapsCompanyDropdownDto()
    {
        var mapper = new MapperConfiguration(configuration => configuration.AddProfile<MappingProfile>()).CreateMapper();
        var company = new Company
        {
            Id = 12,
            Name = "Mapped Company",
            City = "Prishtine",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = mapper.Map<CompanyDropdownDto>(company);

        Assert.Equal(company.Id, result.Id);
        Assert.Equal(company.Name, result.Name);
        Assert.Equal(company.City, result.City);
        Assert.Equal(company.IsActive, result.IsActive);
    }

    private static CompanyService CreateService(AppDbContext dbContext)
    {
        var mapper = new MapperConfiguration(configuration => configuration.AddProfile<MappingProfile>()).CreateMapper();

        return new CompanyService(
            new CompanyRepository(dbContext),
            mapper,
            NullLogger<CompanyService>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedCompaniesAsync(AppDbContext dbContext, params Company[] companies)
    {
        dbContext.Companies.AddRange(companies);
        await dbContext.SaveChangesAsync();
    }
}
