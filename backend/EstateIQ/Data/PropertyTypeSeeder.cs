using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class PropertyTypeSeeder
{
    private static readonly string[] RequiredPropertyTypes =
    [
        "House",
        "Apartment",
        "Villa",
        "Land",
        "Commercial"
    ];

    public static async Task SeedRequiredPropertyTypesAsync(AppDbContext dbContext)
    {
        var existingNames = await dbContext.PropertyTypes
            .AsNoTracking()
            .Select(propertyType => propertyType.Name)
            .ToListAsync();

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingTypes = RequiredPropertyTypes
            .Where(required => !existingSet.Contains(required))
            .Select(name => new PropertyType
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingTypes.Count == 0)
        {
            return;
        }

        dbContext.PropertyTypes.AddRange(missingTypes);
        await dbContext.SaveChangesAsync();
    }
}
