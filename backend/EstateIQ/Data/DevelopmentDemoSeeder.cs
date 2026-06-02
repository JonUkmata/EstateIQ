using EstateIQ.Constants;
using EstateIQ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class DevelopmentDemoSeeder
{
    private const string DemoPassword = "Demo123!";

    private static readonly DemoCompany[] Companies =
    [
        new("Prime Real Estate", "info@primerealestate.local", "+383 44 100 200", "Rruga B", "Prishtine", "https://prime-estate.local"),
        new("City Properties", "hello@cityproperties.local", "+355 69 200 300", "Bulevardi Deshmoret e Kombit", "Tirane", "https://city-properties.local"),
        new("Urban Living Group", "contact@urbanliving.local", "+383 45 300 400", "Lagjja Marigona", "Prishtine", "https://urban-living.local"),
        new("Adria Homes", "sales@adriahomes.local", "+355 68 400 500", "Lungomare", "Vlore", "https://adria-homes.local")
    ];

    private static readonly DemoUser[] Users =
    [
        new("admin@estateiq.local", "System", "Admin", Roles.Admin, null),
        new("admin.presentation@estateiq.local", "Presentation", "Admin", Roles.Admin, null),
        new("prime.admin@estateiq.local", "Prime", "Admin", Roles.CompanyAdmin, "Prime Real Estate"),
        new("city.admin@estateiq.local", "City", "Admin", Roles.CompanyAdmin, "City Properties"),
        new("urban.admin@estateiq.local", "Urban", "Admin", Roles.CompanyAdmin, "Urban Living Group"),
        new("company.presentation@estateiq.local", "Presentation", "Company Admin", Roles.CompanyAdmin, "Prime Real Estate"),
        new("ardit.hoxha@estateiq.local", "Ardit", "Hoxha", Roles.Agent, "Prime Real Estate"),
        new("lea.krasniqi@estateiq.local", "Lea", "Krasniqi", Roles.Agent, "Prime Real Estate"),
        new("mira.berisha@estateiq.local", "Mira", "Berisha", Roles.Agent, "City Properties"),
        new("dren.gashi@estateiq.local", "Dren", "Gashi", Roles.Agent, "Urban Living Group"),
        new("nora.shala@estateiq.local", "Nora", "Shala", Roles.Agent, "Adria Homes"),
        new("agent.presentation@estateiq.local", "Presentation", "Agent", Roles.Agent, "Prime Real Estate"),
        new("user.presentation@estateiq.local", "Presentation", "User", Roles.User, null),
        new("demo.user@estateiq.local", "Demo", "User", Roles.User, null)
    ];

    private static readonly DemoProperty[] Properties =
    [
        new("Prime Tower Apartment", 145000m, 88m, "Rruga B", "Prishtine", "Apartment", "For Sale", "Prime Real Estate", "ardit.hoxha@estateiq.local", 2, 1, 5, 2022, 42.64880000m, 21.16490000m),
        new("Marigona Family Villa", 385000m, 245m, "Lagjja Marigona", "Prishtine", "Villa", "For Sale", "Urban Living Group", "dren.gashi@estateiq.local", 4, 3, 2, 2021, 42.59630000m, 21.11310000m),
        new("Blloku Premium Flat", 219000m, 112m, "Rruga Pjeter Bogdani", "Tirane", "Apartment", "For Sale", "City Properties", "mira.berisha@estateiq.local", 3, 2, 6, 2020, 41.32290000m, 19.81170000m),
        new("Vlore Sea View Penthouse", 410000m, 190m, "Lungomare", "Vlore", "Penthouse", "For Sale", "Adria Homes", "nora.shala@estateiq.local", 3, 2, 9, 2023, 40.45290000m, 19.48660000m),
        new("Prime Business Office", 260000m, 138m, "Rruga Rexhep Luci", "Prishtine", "Office", "For Rent", "Prime Real Estate", "lea.krasniqi@estateiq.local", 0, 2, 4, 2018, 42.66290000m, 21.16550000m),
        new("Tirana Central Office", 310000m, 165m, "Rruga Ibrahim Rugova", "Tirane", "Office", "For Rent", "City Properties", "mira.berisha@estateiq.local", 0, 2, 3, 2017, 41.31860000m, 19.82140000m),
        new("Urban Garden House", 235000m, 172m, "Rruga e Elbasanit", "Tirane", "House", "Under Contract", "City Properties", "mira.berisha@estateiq.local", 4, 2, 2, 2016, 41.31670000m, 19.83610000m),
        new("Sunny Duplex Prishtine", 198000m, 128m, "Rruga Muharrem Fejza", "Prishtine", "House", "Sold", "Prime Real Estate", "ardit.hoxha@estateiq.local", 3, 2, 2, 2019, 42.64690000m, 21.17220000m),
        new("Adria Beach Apartment", 176000m, 92m, "Shkembi i Kavajes", "Durres", "Apartment", "For Sale", "Adria Homes", "nora.shala@estateiq.local", 2, 1, 7, 2022, 41.27520000m, 19.51660000m),
        new("Urban Retail Corner", 225000m, 118m, "Rruga Nena Tereze", "Gjakove", "Commercial", "For Rent", "Urban Living Group", "dren.gashi@estateiq.local", 0, 1, 1, 2015, 42.38170000m, 20.42860000m),
        new("Prime Lake Residence", 168000m, 96m, "Rruga Liqeni", "Prishtine", "Apartment", "Rented", "Prime Real Estate", "lea.krasniqi@estateiq.local", 2, 1, 5, 2020, 42.65350000m, 21.14180000m),
        new("City Compact Studio", 79000m, 42m, "Rruga Myslym Shyri", "Tirane", "Apartment", "For Sale", "City Properties", "mira.berisha@estateiq.local", 1, 1, 2, 2014, 41.32640000m, 19.81190000m),
        new("Adria Hillside Villa", 365000m, 230m, "Rruga e Beratit", "Vlore", "Villa", "Under Contract", "Adria Homes", "nora.shala@estateiq.local", 4, 3, 2, 2021, 40.47050000m, 19.49440000m),
        new("Urban New Build", 152000m, 86m, "Rruga C", "Prishtine", "Apartment", "For Sale", "Urban Living Group", "dren.gashi@estateiq.local", 2, 1, 6, 2024, 42.65050000m, 21.16390000m),
        new("Prime City Studio", 92000m, 48m, "Rruga Garibaldi", "Prishtine", "Apartment", "Sold", "Prime Real Estate", "ardit.hoxha@estateiq.local", 1, 1, 3, 2013, 42.66130000m, 21.15970000m),
        new("City Luxury Penthouse", 470000m, 205m, "Bulevardi Zogu I", "Tirane", "Penthouse", "For Sale", "City Properties", "mira.berisha@estateiq.local", 4, 3, 10, 2023, 41.33360000m, 19.81660000m),
        new("Presentation Skyline Penthouse", 520000m, 214m, "Rruga B", "Prishtine", "Penthouse", "For Sale", "Prime Real Estate", "agent.presentation@estateiq.local", 4, 3, 12, 2024, 42.64940000m, 21.16530000m, 1),
        new("Presentation Lake Apartment", 182000m, 96m, "Rruga Liqeni", "Prishtine", "Apartment", "For Sale", "Prime Real Estate", "agent.presentation@estateiq.local", 2, 2, 6, 2021, 42.65320000m, 21.14240000m, 2),
        new("Presentation Family Residence", 295000m, 188m, "Lagjja Marigona", "Prishtine", "House", "For Sale", "Prime Real Estate", "agent.presentation@estateiq.local", 4, 3, 2, 2020, 42.59720000m, 21.11420000m, 3),
        new("Presentation Business Suite", 245000m, 132m, "Rruga Rexhep Luci", "Prishtine", "Office", "For Rent", "Prime Real Estate", "agent.presentation@estateiq.local", 0, 2, 5, 2019, 42.66350000m, 21.16480000m, 4),
        new("Presentation Retail Gallery", 198000m, 112m, "Rruga UCK", "Prishtine", "Commercial", "For Rent", "Prime Real Estate", "agent.presentation@estateiq.local", 0, 1, 1, 2018, 42.66420000m, 21.15870000m, 5),
        new("Presentation Sold Duplex", 225000m, 146m, "Rruga Muharrem Fejza", "Prishtine", "House", "Sold", "Prime Real Estate", "agent.presentation@estateiq.local", 3, 2, 2, 2017, 42.64650000m, 21.17280000m, 6),
        new("Presentation Rented Studio", 86000m, 44m, "Rruga Garibaldi", "Prishtine", "Apartment", "Rented", "Prime Real Estate", "agent.presentation@estateiq.local", 1, 1, 3, 2016, 42.66160000m, 21.16010000m, 7),
        new("Presentation Contract Villa", 410000m, 236m, "Veternik", "Prishtine", "Villa", "Under Contract", "Prime Real Estate", "agent.presentation@estateiq.local", 5, 3, 2, 2022, 42.62580000m, 21.16420000m, 8),
        new("Presentation Off Market Land", 155000m, 920m, "Matiqan", "Prishtine", "Land", "Off Market", "Prime Real Estate", "agent.presentation@estateiq.local", 0, 1, 1, 2015, 42.64270000m, 21.19310000m, 9),
        new("Presentation Premium Loft", 167000m, 82m, "Ulpiana", "Prishtine", "Apartment", "For Sale", "Prime Real Estate", "agent.presentation@estateiq.local", 2, 1, 4, 2023, 42.65480000m, 21.15990000m, 10)
    ];

    public static async Task SeedAsync(AppDbContext dbContext, string contentRootPath)
    {
        await SeedCompaniesAsync(dbContext);
        await SeedUsersAndRolesAsync(dbContext);
        await SeedAgentProfilesAndCompanyLinksAsync(dbContext);
        await SeedCompanyAdminsAsync(dbContext);
        await SeedPropertiesAsync(dbContext);
        await SeedPropertyImagesAsync(dbContext, contentRootPath);
    }

    private static async Task SeedCompaniesAsync(AppDbContext dbContext)
    {
        var existingCompanies = await dbContext.Companies.ToDictionaryAsync(company => company.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var demo in Companies)
        {
            if (!existingCompanies.TryGetValue(demo.Name, out var company))
            {
                company = new Company
                {
                    Name = demo.Name,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Companies.Add(company);
                existingCompanies[demo.Name] = company;
            }

            company.Email = demo.Email;
            company.Phone = demo.Phone;
            company.Address = demo.Address;
            company.City = demo.City;
            company.Website = demo.Website;
            company.IsActive = true;
            company.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAndRolesAsync(AppDbContext dbContext)
    {
        var passwordHasher = new PasswordHasher<User>();
        var rolesByName = await dbContext.Roles.ToDictionaryAsync(role => role.Name, StringComparer.OrdinalIgnoreCase);
        var usersByEmail = await dbContext.Users.ToDictionaryAsync(user => user.Email, StringComparer.OrdinalIgnoreCase);

        foreach (var demo in Users)
        {
            if (!usersByEmail.TryGetValue(demo.Email, out var user))
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = demo.Email,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(user);
                usersByEmail[demo.Email] = user;
            }

            user.FirstName = demo.FirstName;
            user.LastName = demo.LastName;
            user.IsActive = true;
            user.IsEmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.PasswordHash = passwordHasher.HashPassword(user, DemoPassword);

            var role = rolesByName[demo.Role];
            var hasRole = await dbContext.UserRoles.AnyAsync(userRole => userRole.UserId == user.Id && userRole.RoleId == role.Id);
            if (!hasRole)
            {
                dbContext.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAgentProfilesAndCompanyLinksAsync(AppDbContext dbContext)
    {
        var agentUsers = Users.Where(user => user.Role == Roles.Agent).ToList();
        var usersByEmail = await dbContext.Users.ToDictionaryAsync(user => user.Email, StringComparer.OrdinalIgnoreCase);
        var companiesByName = await dbContext.Companies.ToDictionaryAsync(company => company.Name, StringComparer.OrdinalIgnoreCase);
        var agentsByEmail = await dbContext.Agents.ToDictionaryAsync(agent => agent.Email, StringComparer.OrdinalIgnoreCase);

        foreach (var demo in agentUsers)
        {
            var user = usersByEmail[demo.Email];
            if (!agentsByEmail.TryGetValue(demo.Email, out var agent))
            {
                agent = new Agent
                {
                    Email = demo.Email,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Agents.Add(agent);
                agentsByEmail[demo.Email] = agent;
            }

            agent.UserId = user.Id;
            agent.FirstName = demo.FirstName;
            agent.LastName = demo.LastName;
            agent.Phone = "+383 38 555 100";
            agent.Mobile = "+383 44 555 100";
            agent.Bio = $"Demo agent for {demo.CompanyName}.";
            agent.IsActive = true;
            agent.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        foreach (var demo in agentUsers)
        {
            if (demo.CompanyName is null)
            {
                continue;
            }

            var agentId = agentsByEmail[demo.Email].Id;
            var companyId = companiesByName[demo.CompanyName].Id;
            var exists = await dbContext.AgentCompanies.AnyAsync(link => link.AgentId == agentId && link.CompanyId == companyId);

            if (exists)
            {
                var link = await dbContext.AgentCompanies.FirstAsync(link => link.AgentId == agentId && link.CompanyId == companyId);
                link.IsActive = true;
                link.Role = "Agent";
                link.JoinedDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-8));
                continue;
            }

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                AgentId = agentId,
                CompanyId = companyId,
                Role = "Agent",
                JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-8)),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCompanyAdminsAsync(AppDbContext dbContext)
    {
        var companyAdminUsers = Users.Where(user => user.Role == Roles.CompanyAdmin && user.CompanyName is not null).ToList();
        var usersByEmail = await dbContext.Users.ToDictionaryAsync(user => user.Email, StringComparer.OrdinalIgnoreCase);
        var companiesByName = await dbContext.Companies.ToDictionaryAsync(company => company.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var demo in companyAdminUsers)
        {
            var userId = usersByEmail[demo.Email].Id;
            var companyId = companiesByName[demo.CompanyName!].Id;
            var exists = await dbContext.CompanyUsers.AnyAsync(companyUser => companyUser.CompanyId == companyId && companyUser.UserId == userId);

            if (exists)
            {
                var companyUser = await dbContext.CompanyUsers.FirstAsync(companyUser => companyUser.CompanyId == companyId && companyUser.UserId == userId);
                companyUser.RelationshipType = Roles.CompanyAdmin;
                continue;
            }

            dbContext.CompanyUsers.Add(new CompanyUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                UserId = userId,
                RelationshipType = Roles.CompanyAdmin,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPropertiesAsync(AppDbContext dbContext)
    {
        var companiesByName = await dbContext.Companies.ToDictionaryAsync(company => company.Name, StringComparer.OrdinalIgnoreCase);
        var agentsByEmail = await dbContext.Agents.ToDictionaryAsync(agent => agent.Email, StringComparer.OrdinalIgnoreCase);
        var typesByName = await dbContext.PropertyTypes.ToDictionaryAsync(type => type.Name, StringComparer.OrdinalIgnoreCase);
        var statusesByName = await dbContext.PropertyStatuses.ToDictionaryAsync(status => status.Name, StringComparer.OrdinalIgnoreCase);
        var existingByTitle = await dbContext.Properties.ToDictionaryAsync(property => property.Title, StringComparer.OrdinalIgnoreCase);

        foreach (var demo in Properties)
        {
            if (!existingByTitle.TryGetValue(demo.Title, out var property))
            {
                property = new Property
                {
                    Title = demo.Title,
                    CreatedAt = DateTime.UtcNow.AddDays(-demo.DaysAgo)
                };
                dbContext.Properties.Add(property);
                existingByTitle[demo.Title] = property;
            }

            property.Description = $"{demo.Title} in {demo.City}, prepared as clean demo data for richer dashboards.";
            property.Price = demo.Price;
            property.Area = demo.Area;
            property.Address = demo.Address;
            property.City = demo.City;
            property.Bedrooms = demo.Bedrooms == 0 ? null : demo.Bedrooms;
            property.Bathrooms = demo.Bathrooms;
            property.Floors = demo.Floors;
            property.YearBuilt = demo.YearBuilt;
            property.Latitude = demo.Latitude;
            property.Longitude = demo.Longitude;
            property.PropertyTypeId = typesByName[demo.TypeName].Id;
            property.PropertyStatusId = statusesByName[demo.StatusName].Id;
            property.CompanyId = companiesByName[demo.CompanyName].Id;
            property.AgentId = agentsByEmail[demo.AgentEmail].Id;
            property.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPropertyImagesAsync(AppDbContext dbContext, string contentRootPath)
    {
        const string propertyEntity = "Property";
        const int imagesPerProperty = 2;

        var properties = await dbContext.Properties
            .AsNoTracking()
            .OrderBy(property => property.Id)
            .Select(property => new
            {
                property.Id,
                property.Title,
                property.City,
                property.Price,
                Type = property.PropertyType.Name,
                Status = property.PropertyStatus.Name
            })
            .ToListAsync();

        if (properties.Count == 0)
        {
            return;
        }

        var webRootPath = Path.Combine(contentRootPath, "wwwroot");
        var recordsToInsert = new List<FileRecord>();

        foreach (var property in properties)
        {
            var entityId = CreatePropertyEntityId(property.Id);
            var existingCount = await dbContext.Files
                .AsNoTracking()
                .CountAsync(file => file.Entity == propertyEntity && file.EntityId == entityId);

            if (existingCount > 0)
            {
                continue;
            }

            var uploadDirectory = Path.Combine(webRootPath, "uploads", "properties", property.Id.ToString());
            Directory.CreateDirectory(uploadDirectory);

            for (var index = 1; index <= imagesPerProperty; index++)
            {
                var fileName = $"demo-{index}.svg";
                var absolutePath = Path.Combine(uploadDirectory, fileName);
                var svg = BuildPropertyImageSvg(property.Id, property.Title, property.City, property.Type, property.Status, index);

                await File.WriteAllTextAsync(absolutePath, svg);

                var fileInfo = new FileInfo(absolutePath);
                recordsToInsert.Add(new FileRecord
                {
                    Id = Guid.NewGuid(),
                    Entity = propertyEntity,
                    EntityId = entityId,
                    FileName = fileName,
                    FilePath = $"/uploads/properties/{property.Id}/{fileName}",
                    ContentType = "image/svg+xml",
                    FileSize = fileInfo.Length,
                    CreatedAt = DateTime.UtcNow.AddSeconds(index)
                });
            }
        }

        if (recordsToInsert.Count > 0)
        {
            dbContext.Files.AddRange(recordsToInsert);
            await dbContext.SaveChangesAsync();
        }
    }

    private static string BuildPropertyImageSvg(int propertyId, string title, string city, string type, string status, int variant)
    {
        var palette = GetImagePalette(propertyId + variant);
        var escapedTitle = EscapeXml(title);
        var escapedCity = EscapeXml(city);
        var escapedType = EscapeXml(type);
        var escapedStatus = EscapeXml(status);

        return $$"""
            <svg xmlns="http://www.w3.org/2000/svg" width="1280" height="820" viewBox="0 0 1280 820" role="img" aria-label="{{escapedTitle}}">
              <defs>
                <linearGradient id="sky" x1="0" x2="1" y1="0" y2="1">
                  <stop offset="0" stop-color="{{palette.SkyStart}}"/>
                  <stop offset="1" stop-color="{{palette.SkyEnd}}"/>
                </linearGradient>
                <linearGradient id="glass" x1="0" x2="1" y1="0" y2="1">
                  <stop offset="0" stop-color="{{palette.GlassStart}}"/>
                  <stop offset="1" stop-color="{{palette.GlassEnd}}"/>
                </linearGradient>
              </defs>
              <rect width="1280" height="820" fill="url(#sky)"/>
              <circle cx="{{(variant == 1 ? 1010 : 220)}}" cy="150" r="72" fill="#fff4b8" opacity=".82"/>
              <path d="M0 610 C180 560 290 590 430 548 C590 500 730 555 890 514 C1050 474 1160 506 1280 470 L1280 820 L0 820 Z" fill="{{palette.Hills}}" opacity=".9"/>
              <path d="M0 665 C160 620 330 642 500 606 C710 562 860 650 1040 594 C1130 565 1215 576 1280 590 L1280 820 L0 820 Z" fill="{{palette.Ground}}"/>
              <rect x="192" y="255" width="760" height="380" rx="10" fill="{{palette.Building}}" stroke="#ffffff" stroke-opacity=".48" stroke-width="5"/>
              <rect x="258" y="198" width="356" height="438" rx="8" fill="{{palette.BuildingDark}}" stroke="#ffffff" stroke-opacity=".32" stroke-width="4"/>
              <rect x="676" y="318" width="360" height="318" rx="8" fill="{{palette.BuildingAlt}}" stroke="#ffffff" stroke-opacity=".35" stroke-width="4"/>
              <path d="M232 255 L435 116 L640 255 Z" fill="{{palette.Roof}}"/>
              <path d="M650 318 L855 196 L1066 318 Z" fill="{{palette.RoofAlt}}"/>
              <g opacity=".92">
                <rect x="304" y="278" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="422" y="278" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="540" y="278" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="304" y="374" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="422" y="374" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="540" y="374" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="304" y="470" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="422" y="470" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="540" y="470" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="724" y="372" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="840" y="372" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="724" y="472" width="74" height="58" rx="6" fill="url(#glass)"/>
                <rect x="840" y="472" width="74" height="58" rx="6" fill="url(#glass)"/>
              </g>
              <rect x="468" y="535" width="118" height="101" rx="8" fill="{{palette.Door}}"/>
              <path d="M150 636 H1120" stroke="#ffffff" stroke-opacity=".55" stroke-width="7"/>
              <rect x="74" y="76" width="430" height="136" rx="8" fill="#111827" opacity=".76"/>
              <text x="108" y="130" font-family="Inter, Arial, sans-serif" font-size="34" font-weight="700" fill="#ffffff">{{escapedTitle}}</text>
              <text x="108" y="174" font-family="Inter, Arial, sans-serif" font-size="24" fill="#dbeafe">{{escapedCity}} · {{escapedType}}</text>
              <rect x="982" y="78" width="206" height="58" rx="8" fill="#ffffff" opacity=".9"/>
              <text x="1010" y="116" font-family="Inter, Arial, sans-serif" font-size="24" font-weight="700" fill="#111827">{{escapedStatus}}</text>
              <text x="78" y="760" font-family="Inter, Arial, sans-serif" font-size="20" fill="#ffffff" opacity=".82">EstateIQ demo listing image {{variant}}</text>
            </svg>
            """;
    }

    private static (string SkyStart, string SkyEnd, string Hills, string Ground, string Building, string BuildingDark, string BuildingAlt, string Roof, string RoofAlt, string GlassStart, string GlassEnd, string Door) GetImagePalette(int seed)
    {
        return (seed % 5) switch
        {
            0 => ("#8ecae6", "#fef3c7", "#5b8e7d", "#2f6f4e", "#f8fafc", "#dbeafe", "#e2e8f0", "#b91c1c", "#7f1d1d", "#bfdbfe", "#38bdf8", "#78350f"),
            1 => ("#93c5fd", "#fde68a", "#64748b", "#3f6212", "#f1f5f9", "#cbd5e1", "#e5e7eb", "#0f766e", "#115e59", "#dbeafe", "#60a5fa", "#374151"),
            2 => ("#bae6fd", "#fed7aa", "#6b7280", "#166534", "#fff7ed", "#fed7aa", "#fde68a", "#9a3412", "#7c2d12", "#e0f2fe", "#7dd3fc", "#451a03"),
            3 => ("#a7f3d0", "#bfdbfe", "#4b5563", "#15803d", "#f8fafc", "#d1d5db", "#e0f2fe", "#1d4ed8", "#1e3a8a", "#eff6ff", "#93c5fd", "#1f2937"),
            _ => ("#c4b5fd", "#fbcfe8", "#64748b", "#4d7c0f", "#fdf2f8", "#fce7f3", "#f3e8ff", "#be123c", "#881337", "#f5d0fe", "#c084fc", "#581c87")
        };
    }

    private static Guid CreatePropertyEntityId(int propertyId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(propertyId).CopyTo(bytes, 0);

        return new Guid(bytes);
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
    }

    private sealed record DemoCompany(string Name, string Email, string Phone, string Address, string City, string Website);

    private sealed record DemoUser(string Email, string FirstName, string LastName, string Role, string? CompanyName);

    private sealed record DemoProperty(
        string Title,
        decimal Price,
        decimal Area,
        string Address,
        string City,
        string TypeName,
        string StatusName,
        string CompanyName,
        string AgentEmail,
        int Bedrooms,
        int Bathrooms,
        int Floors,
        int YearBuilt,
        decimal Latitude,
        decimal Longitude,
        int DaysAgo = 0);
}
