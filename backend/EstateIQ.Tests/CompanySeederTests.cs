using EstateIQ.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class CompanySeederTests
{
    [Fact]
    public async Task SeedRequiredCompaniesAsync_InsertsRequiredCompanies()
    {
        await using var dbContext = CreateContext();

        await CompanySeeder.SeedRequiredCompaniesAsync(dbContext);

        var companyNames = await dbContext.Companies
            .Select(company => company.Name)
            .ToListAsync();

        Assert.Contains("Prime Real Estate", companyNames);
        Assert.Contains("City Properties", companyNames);
        Assert.Contains("Urban Living Group", companyNames);
        Assert.True(companyNames.Count >= 3);
    }

    [Fact]
    public async Task SeedRequiredCompaniesAsync_DoesNotCreateDuplicates()
    {
        await using var dbContext = CreateContext();

        await CompanySeeder.SeedRequiredCompaniesAsync(dbContext);
        await CompanySeeder.SeedRequiredCompaniesAsync(dbContext);

        var duplicateNames = await dbContext.Companies
            .GroupBy(company => company.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToListAsync();

        Assert.Empty(duplicateNames);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
