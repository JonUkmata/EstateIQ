using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class CompanySeeder
{
    private static readonly string[] RequiredCompanies =
    [
        "Prime Real Estate",
        "City Properties",
        "Urban Living Group"
    ];

    public static async Task SeedRequiredCompaniesAsync(AppDbContext dbContext)
    {
        var existingNames = await dbContext.Companies
            .AsNoTracking()
            .Select(company => company.Name)
            .ToListAsync();

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingCompanies = RequiredCompanies
            .Where(required => !existingSet.Contains(required))
            .Select(name => new Company
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingCompanies.Count == 0)
        {
            return;
        }

        dbContext.Companies.AddRange(missingCompanies);
        await dbContext.SaveChangesAsync();
    }
}
