using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class PropertySeeder
{
    private static readonly (string Title, decimal Price, decimal Area, string Address, string City)[] RequiredProperties =
    [
        ("Modern Apartment", 120000m, 78m, "Rruga e Kavajes", "Tirane"),
        ("Family House", 185000m, 142m, "Rruga Muhamet Gjollesha", "Tirane"),
        ("Coastal Villa", 320000m, 210m, "Lungomare", "Vlore"),
        ("Downtown Studio", 85000m, 44m, "Bulevardi Deshmoret e Kombit", "Tirane"),
        ("Lake View Apartment", 148000m, 92m, "Rruga Pjeter Bogdani", "Tirane"),
        ("Suburban House", 210000m, 168m, "Rruga e Elbasanit", "Tirane"),
        ("Business Office", 260000m, 135m, "Rruga Ibrahim Rugova", "Prishtine"),
        ("City Center Flat", 99000m, 63m, "Rruga Garibaldi", "Prishtine"),
        ("Garden Villa", 410000m, 260m, "Lagjja Marigona", "Prishtine"),
        ("Beach Apartment", 175000m, 88m, "Shkembi i Kavajes", "Durres"),
        ("Seaside Penthouse", 390000m, 185m, "Rruga Taulantia", "Durres"),
        ("Old Town House", 135000m, 118m, "Rruga Kol Idromeno", "Shkoder"),
        ("Mountain Cabin", 98000m, 96m, "Rruga e Thethit", "Shkoder"),
        ("Central Office Space", 225000m, 122m, "Rruga Adem Jashari", "Peje"),
        ("Family Apartment", 112000m, 74m, "Rruga Bill Clinton", "Ferizaj"),
        ("Commercial Unit", 305000m, 156m, "Rruga Ahmet Kaciku", "Ferizaj"),
        ("Hillside Villa", 360000m, 238m, "Rruga e Beratit", "Vlore"),
        ("Compact Studio", 69000m, 38m, "Rruga Ismail Qemali", "Vlore"),
        ("Modern Duplex", 245000m, 152m, "Rruga Skenderbeu", "Gjakove"),
        ("Retail Space", 198000m, 108m, "Rruga Nena Tereze", "Gjakove"),
        ("Urban Loft", 155000m, 84m, "Rruga Qemal Stafa", "Tirane"),
        ("New Build Apartment", 132000m, 80m, "Rruga B", "Prishtine")
    ];

    public static async Task SeedRequiredPropertiesAsync(AppDbContext dbContext)
    {
        var propertyTypeIds = await dbContext.PropertyTypes
            .AsNoTracking()
            .OrderBy(propertyType => propertyType.Id)
            .Select(propertyType => propertyType.Id)
            .ToListAsync();

        var propertyStatusIds = await dbContext.PropertyStatuses
            .AsNoTracking()
            .OrderBy(propertyStatus => propertyStatus.Id)
            .Select(propertyStatus => propertyStatus.Id)
            .ToListAsync();

        var relationships = await dbContext.AgentCompanies
            .AsNoTracking()
            .Where(agentCompany => agentCompany.IsActive)
            .OrderBy(agentCompany => agentCompany.Id)
            .Select(agentCompany => new
            {
                agentCompany.AgentId,
                agentCompany.CompanyId
            })
            .ToListAsync();

        if (propertyTypeIds.Count == 0 || propertyStatusIds.Count == 0 || relationships.Count == 0)
        {
            return;
        }

        var existingTitles = await dbContext.Properties
            .AsNoTracking()
            .Select(property => property.Title)
            .ToListAsync();

        var existingTitleSet = existingTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var properties = RequiredProperties
            .Where(property => !existingTitleSet.Contains(property.Title))
            .Select((property, index) => (Property: property, Index: index))
            .Select(property => new Property
            {
                Title = property.Property.Title,
                Description = $"{property.Property.Title} listed in {property.Property.City}.",
                Price = property.Property.Price,
                Area = property.Property.Area,
                Bedrooms = 1 + property.Index % 5,
                Bathrooms = 1 + property.Index % 3,
                Floors = 1 + property.Index % 4,
                YearBuilt = 2021,
                PropertyTypeId = propertyTypeIds[property.Index % propertyTypeIds.Count],
                PropertyStatusId = propertyStatusIds[property.Index % propertyStatusIds.Count],
                CompanyId = relationships[property.Index % relationships.Count].CompanyId,
                AgentId = relationships[property.Index % relationships.Count].AgentId,
                Address = property.Property.Address,
                City = property.Property.City,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (properties.Count == 0)
        {
            return;
        }

        dbContext.Properties.AddRange(properties);
        await dbContext.SaveChangesAsync();
    }
}
