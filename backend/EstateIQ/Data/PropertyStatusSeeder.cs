using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class PropertyStatusSeeder
{
    private static readonly (string Name, string ColorCode)[] RequiredStatuses =
    [
        ("Available", "#007bff"),
        ("Pending", "#ffc107"),
        ("Sold", "#28a745")
    ];

    public static async Task SeedRequiredPropertyStatusesAsync(AppDbContext dbContext)
    {
        var existingNames = await dbContext.PropertyStatuses
            .AsNoTracking()
            .Select(status => status.Name)
            .ToListAsync();

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingStatuses = RequiredStatuses
            .Where(required => !existingSet.Contains(required.Name))
            .Select(required => new PropertyStatus
            {
                Name = required.Name,
                ColorCode = required.ColorCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingStatuses.Count == 0)
        {
            return;
        }

        dbContext.PropertyStatuses.AddRange(missingStatuses);
        await dbContext.SaveChangesAsync();
    }
}
