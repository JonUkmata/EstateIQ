using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class PropertySeeder
{
    private static readonly (string Title, decimal Price, decimal Area, string Address, string City, decimal Latitude, decimal Longitude)[] RequiredProperties =
    [
        ("Modern Apartment", 120000m, 78m, "Rruga e Kavajes", "Tirane", 41.32750000m, 19.81870000m),
        ("Family House", 185000m, 142m, "Rruga Muhamet Gjollesha", "Tirane", 41.33180000m, 19.80690000m),
        ("Coastal Villa", 320000m, 210m, "Lungomare", "Vlore", 40.45290000m, 19.48660000m),
        ("Downtown Studio", 85000m, 44m, "Bulevardi Deshmoret e Kombit", "Tirane", 41.31860000m, 19.82140000m),
        ("Lake View Apartment", 148000m, 92m, "Rruga Pjeter Bogdani", "Tirane", 41.32290000m, 19.81170000m),
        ("Suburban House", 210000m, 168m, "Rruga e Elbasanit", "Tirane", 41.31670000m, 19.83610000m),
        ("Business Office", 260000m, 135m, "Rruga Ibrahim Rugova", "Prishtine", 42.66290000m, 21.16550000m),
        ("City Center Flat", 99000m, 63m, "Rruga Garibaldi", "Prishtine", 42.66130000m, 21.15970000m),
        ("Garden Villa", 410000m, 260m, "Lagjja Marigona", "Prishtine", 42.59630000m, 21.11310000m),
        ("Beach Apartment", 175000m, 88m, "Shkembi i Kavajes", "Durres", 41.27520000m, 19.51660000m),
        ("Seaside Penthouse", 390000m, 185m, "Rruga Taulantia", "Durres", 41.31330000m, 19.44690000m),
        ("Old Town House", 135000m, 118m, "Rruga Kol Idromeno", "Shkoder", 42.06830000m, 19.51260000m),
        ("Mountain Cabin", 98000m, 96m, "Rruga e Thethit", "Shkoder", 42.39590000m, 19.77450000m),
        ("Central Office Space", 225000m, 122m, "Rruga Adem Jashari", "Peje", 42.65910000m, 20.28830000m),
        ("Family Apartment", 112000m, 74m, "Rruga Bill Clinton", "Ferizaj", 42.37020000m, 21.15500000m),
        ("Commercial Unit", 305000m, 156m, "Rruga Ahmet Kaciku", "Ferizaj", 42.37160000m, 21.15330000m),
        ("Hillside Villa", 360000m, 238m, "Rruga e Beratit", "Vlore", 40.47050000m, 19.49440000m),
        ("Compact Studio", 69000m, 38m, "Rruga Ismail Qemali", "Vlore", 40.46610000m, 19.49140000m),
        ("Modern Duplex", 245000m, 152m, "Rruga Skenderbeu", "Gjakove", 42.38030000m, 20.43080000m),
        ("Retail Space", 198000m, 108m, "Rruga Nena Tereze", "Gjakove", 42.38170000m, 20.42860000m),
        ("Urban Loft", 155000m, 84m, "Rruga Qemal Stafa", "Tirane", 41.32920000m, 19.82460000m),
        ("New Build Apartment", 132000m, 80m, "Rruga B", "Prishtine", 42.64880000m, 21.16490000m)
    ];

    private static readonly Dictionary<string, (decimal Latitude, decimal Longitude)> CityCoordinates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tirane"] = (41.32750000m, 19.81870000m),
        ["Tirana"] = (41.32750000m, 19.81870000m),
        ["Prishtine"] = (42.66290000m, 21.16550000m),
        ["Prishtina"] = (42.66290000m, 21.16550000m),
        ["Vlore"] = (40.46610000m, 19.49140000m),
        ["Durres"] = (41.31330000m, 19.44690000m),
        ["Shkoder"] = (42.06830000m, 19.51260000m),
        ["Peje"] = (42.65910000m, 20.28830000m),
        ["Ferizaj"] = (42.37020000m, 21.15500000m),
        ["Gjakove"] = (42.38030000m, 20.43080000m),
        ["Gjakovë"] = (42.38030000m, 20.43080000m)
    };

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
        await BackfillCoordinatesAsync(dbContext);

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
                Latitude = property.Property.Latitude,
                Longitude = property.Property.Longitude,
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

    private static async Task BackfillCoordinatesAsync(AppDbContext dbContext)
    {
        var coordinateLookup = RequiredProperties.ToDictionary(
            property => property.Title,
            property => new { property.Latitude, property.Longitude },
            StringComparer.OrdinalIgnoreCase);

        var existingProperties = await dbContext.Properties
            .Where(property => property.Latitude == null || property.Longitude == null)
            .ToListAsync();

        var hasChanges = false;

        foreach (var property in existingProperties)
        {
            if (coordinateLookup.TryGetValue(property.Title, out var titleCoordinates))
            {
                property.Latitude = titleCoordinates.Latitude;
                property.Longitude = titleCoordinates.Longitude;
                property.UpdatedAt = DateTime.UtcNow;
                hasChanges = true;
                continue;
            }

            if (!CityCoordinates.TryGetValue(property.City, out var cityCoordinates))
            {
                continue;
            }

            property.Latitude = cityCoordinates.Latitude;
            property.Longitude = cityCoordinates.Longitude;
            property.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
