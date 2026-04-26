using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class PropertySeeder
{
    private static readonly (string Title, decimal Price, decimal Area, string Address, string City)[] RequiredProperties =
    [
        ("Modern Apartment", 120000m, 78m, "Rruga e Kavajes", "Tirane"),
        ("Family House", 185000m, 142m, "Rruga Muhamet Gjollesha", "Tirane"),
        ("Coastal Villa", 320000m, 210m, "Lungomare", "Vlore")
    ];

    public static async Task SeedRequiredPropertiesAsync(AppDbContext dbContext)
    {
        if (await dbContext.Properties.AnyAsync())
        {
            return;
        }

        var propertyTypeId = await dbContext.PropertyTypes
            .AsNoTracking()
            .OrderBy(propertyType => propertyType.Id)
            .Select(propertyType => propertyType.Id)
            .FirstOrDefaultAsync();

        var propertyStatusId = await dbContext.PropertyStatuses
            .AsNoTracking()
            .OrderBy(propertyStatus => propertyStatus.Id)
            .Select(propertyStatus => propertyStatus.Id)
            .FirstOrDefaultAsync();

        var relationship = await dbContext.AgentCompanies
            .AsNoTracking()
            .Where(agentCompany => agentCompany.IsActive)
            .OrderBy(agentCompany => agentCompany.Id)
            .Select(agentCompany => new
            {
                agentCompany.AgentId,
                agentCompany.CompanyId
            })
            .FirstOrDefaultAsync();

        if (propertyTypeId == 0 || propertyStatusId == 0 || relationship is null)
        {
            return;
        }

        var properties = RequiredProperties
            .Select(property => new Property
            {
                Title = property.Title,
                Description = $"{property.Title} listed in {property.City}.",
                Price = property.Price,
                Area = property.Area,
                Bedrooms = 2,
                Bathrooms = 1,
                Floors = 1,
                YearBuilt = 2021,
                PropertyTypeId = propertyTypeId,
                PropertyStatusId = propertyStatusId,
                CompanyId = relationship.CompanyId,
                AgentId = relationship.AgentId,
                Address = property.Address,
                City = property.City,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        dbContext.Properties.AddRange(properties);
        await dbContext.SaveChangesAsync();
    }
}
