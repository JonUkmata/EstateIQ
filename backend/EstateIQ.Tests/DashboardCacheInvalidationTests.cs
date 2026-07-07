using AutoMapper;
using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.DTOs.Files;
using EstateIQ.DTOs.Users;
using EstateIQ.Interfaces;
using EstateIQ.Mappings;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services;
using EstateIQ.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EstateIQ.Tests;

public class DashboardCacheInvalidationTests
{
    // ── PropertyService ──────────────────────────────────────────────────────

    [Fact]
    public async Task PropertyService_CreateAsync_InvalidatesAllFourDashboardKeys()
    {
        await using var dbContext = CreateContext();
        await SeedPropertyReferenceDataAsync(dbContext);
        var capturing = new CapturingInvalidationService();
        var service = CreatePropertyService(dbContext, capturing);

        await service.CreateAsync(BuildCreatePropertyDto(companyId: 1, agentId: 1));

        Assert.Contains(("property", 1, 1), capturing.PropertyChangeCalls);
    }

    [Fact]
    public async Task PropertyService_UpdateAsync_InvalidatesNewScope()
    {
        await using var dbContext = CreateContext();
        await SeedPropertyReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, companyId: 1, agentId: 1);
        var capturing = new CapturingInvalidationService();
        var service = CreatePropertyService(dbContext, capturing);
        var dto = BuildUpdatePropertyDto(property);

        await service.UpdateAsync(property.Id, dto);

        Assert.Contains(("property", 1, 1), capturing.PropertyChangeCalls);
    }

    [Fact]
    public async Task PropertyService_UpdateAsync_WhenScopeChanges_InvalidatesBothOldAndNewScopes()
    {
        await using var dbContext = CreateContext();
        await SeedPropertyReferenceDataAsync(dbContext, includeSecondAgentCompany: true);
        var property = await SeedPropertyAsync(dbContext, companyId: 1, agentId: 1);
        var capturing = new CapturingInvalidationService();
        var service = CreatePropertyService(dbContext, capturing);
        var dto = BuildUpdatePropertyDto(property);
        dto.AgentId = 2; // change to second agent

        await service.UpdateAsync(property.Id, dto);

        Assert.Contains(("property", 1, 2), capturing.PropertyChangeCalls); // new scope
        Assert.Contains(("property", 1, 1), capturing.PropertyChangeCalls); // old scope
    }

    [Fact]
    public async Task PropertyService_DeleteAsync_InvalidatesPropertyDashboardKeys()
    {
        await using var dbContext = CreateContext();
        await SeedPropertyReferenceDataAsync(dbContext);
        var property = await SeedPropertyAsync(dbContext, companyId: 1, agentId: 1, statusId: 1);
        var capturing = new CapturingInvalidationService();
        var service = CreatePropertyService(dbContext, capturing);

        await service.DeleteAsync(property.Id);

        Assert.Contains(("property", 1, 1), capturing.PropertyChangeCalls);
    }

    // ── UserService ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UserService_CreateAgentAsync_InvalidatesAgentDashboard()
    {
        await using var dbContext = CreateContext();
        await SeedUserReferenceDataAsync(dbContext);
        var capturing = new CapturingInvalidationService();
        var service = CreateUserService(dbContext, capturing);

        await service.CreateAgentAsync(
            new CreateAgentRequestDto
            {
                FirstName = "New",
                LastName = "Agent",
                Email = "newagent@example.com",
                Password = "Test@123",
                Phone = "+383 44 111 222",
                CompanyId = 1
            },
            currentUserId: Guid.NewGuid(),
            isAdmin: true);

        Assert.Contains(("agent", 1), capturing.AgentChangeCalls);
    }

    [Fact]
    public async Task UserService_CreateCompanyAdminAsync_InvalidatesCompanyAdminDashboard()
    {
        await using var dbContext = CreateContext();
        await SeedUserReferenceDataAsync(dbContext);
        var capturing = new CapturingInvalidationService();
        var service = CreateUserService(dbContext, capturing);

        await service.CreateCompanyAdminAsync(new CreateCompanyAdminRequestDto
        {
            FirstName = "Company",
            LastName = "Admin",
            Email = "companyadmin@example.com",
            Password = "Test@123",
            CompanyId = 1
        });

        Assert.Contains(("companyadmin", 1), capturing.CompanyAdminChangeCalls);
    }

    [Fact]
    public async Task UserService_UpdateUserStatus_ForCompanyAdmin_InvalidatesCompanyAdminDashboard()
    {
        await using var dbContext = CreateContext();
        await SeedUserReferenceDataAsync(dbContext);
        var companyAdminUserId = await SeedCompanyAdminUserAsync(dbContext, companyId: 1);
        var capturing = new CapturingInvalidationService();
        var service = CreateUserService(dbContext, capturing);

        await service.UpdateUserStatusAsync(companyAdminUserId, new UpdateUserStatusRequestDto { IsActive = false });

        Assert.Contains(("companyadmin", 1), capturing.CompanyAdminChangeCalls);
        Assert.Empty(capturing.AgentChangeCalls);
        Assert.Empty(capturing.UserChangeCalls);
    }

    [Fact]
    public async Task UserService_UpdateUserStatus_ForAgent_InvalidatesAgentDashboard()
    {
        await using var dbContext = CreateContext();
        await SeedUserReferenceDataAsync(dbContext);
        var agentUserId = await SeedAgentUserAsync(dbContext, companyId: 1);
        var capturing = new CapturingInvalidationService();
        var service = CreateUserService(dbContext, capturing);

        await service.UpdateUserStatusAsync(agentUserId, new UpdateUserStatusRequestDto { IsActive = false });

        Assert.Contains(("agent", 1), capturing.AgentChangeCalls);
        Assert.Empty(capturing.CompanyAdminChangeCalls);
        Assert.Empty(capturing.UserChangeCalls);
    }

    [Fact]
    public async Task UserService_UpdateUserStatus_ForRegularUser_InvalidatesOnlyAdminDashboard()
    {
        await using var dbContext = CreateContext();
        await SeedUserReferenceDataAsync(dbContext);
        var regularUserId = await SeedRegularUserAsync(dbContext);
        var capturing = new CapturingInvalidationService();
        var service = CreateUserService(dbContext, capturing);

        await service.UpdateUserStatusAsync(regularUserId, new UpdateUserStatusRequestDto { IsActive = false });

        Assert.Single(capturing.UserChangeCalls);
        Assert.Empty(capturing.AgentChangeCalls);
        Assert.Empty(capturing.CompanyAdminChangeCalls);
    }

    // ── Service factories ─────────────────────────────────────────────────────

    private static PropertyService CreatePropertyService(
        AppDbContext dbContext,
        IDashboardInvalidationService invalidation)
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        return new PropertyService(
            new PropertyRepository(dbContext),
            new PropertyTypeRepository(dbContext),
            new PropertyStatusRepository(dbContext),
            new CompanyRepository(dbContext),
            new AgentRepository(dbContext),
            new AgentCompanyRepository(dbContext),
            new NullPropertyImageService(),
            mapper,
            NullLogger<PropertyService>.Instance,
            invalidation);
    }

    private static UserService CreateUserService(
        AppDbContext dbContext,
        IDashboardInvalidationService invalidation)
    {
        return new UserService(
            new UserRepository(dbContext),
            new PasswordService(),
            invalidation,
            new AgentRepository(dbContext));
    }

    // ── Capturing invalidation service ────────────────────────────────────────

    private sealed class CapturingInvalidationService : IDashboardInvalidationService
    {
        public List<(string Type, int CompanyId, int AgentId)> PropertyChangeCalls { get; } = [];
        public List<(string Type, int CompanyId)> AgentChangeCalls { get; } = [];
        public List<(string Type, int CompanyId)> CompanyAdminChangeCalls { get; } = [];
        public List<string> UserChangeCalls { get; } = [];

        public Task InvalidateDashboardsForPropertyChangeAsync(int companyId, int agentId)
        {
            PropertyChangeCalls.Add(("property", companyId, agentId));
            return Task.CompletedTask;
        }

        public Task InvalidateDashboardsForAgentChangeAsync(int companyId)
        {
            AgentChangeCalls.Add(("agent", companyId));
            return Task.CompletedTask;
        }

        public Task InvalidateDashboardsForCompanyAdminChangeAsync(int companyId)
        {
            CompanyAdminChangeCalls.Add(("companyadmin", companyId));
            return Task.CompletedTask;
        }

        public Task InvalidateDashboardsForUserChangeAsync()
        {
            UserChangeCalls.Add("user");
            return Task.CompletedTask;
        }
    }

    private sealed class NullPropertyImageService : IPropertyImageService
    {
        public Task<IReadOnlyList<UploadedFileDto>> UploadImagesAsync(int propertyId, IReadOnlyCollection<IFormFile> files, Guid? uploadedBy)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<FileResponseDto>> GetImagesAsync(int propertyId)
            => Task.FromResult<IReadOnlyList<FileResponseDto>>([]);

        public Task<IReadOnlyDictionary<int, string>> GetCoverImageUrlsAsync(IReadOnlyCollection<int> propertyIds)
            => Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>());

        public Task DeleteImageAsync(int propertyId, Guid imageId)
            => throw new NotSupportedException();
    }

    // ── DB helpers ────────────────────────────────────────────────────────────

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedPropertyReferenceDataAsync(
        AppDbContext dbContext,
        bool includeSecondAgentCompany = false)
    {
        dbContext.PropertyTypes.Add(new PropertyType { Id = 1, Name = "Apartment", IsActive = true, CreatedAt = DateTime.UtcNow });
        dbContext.PropertyStatuses.AddRange(
            new PropertyStatus { Id = 1, Name = "For Sale", IsActive = true, CreatedAt = DateTime.UtcNow },
            new PropertyStatus { Id = 2, Name = "Sold", IsActive = true, CreatedAt = DateTime.UtcNow },
            new PropertyStatus { Id = 3, Name = "Rented", IsActive = true, CreatedAt = DateTime.UtcNow },
            new PropertyStatus { Id = 4, Name = "Under Contract", IsActive = true, CreatedAt = DateTime.UtcNow });
        dbContext.Companies.Add(new Company { Id = 1, Name = "EstateIQ", IsActive = true, CreatedAt = DateTime.UtcNow });
        dbContext.Agents.Add(new Agent { Id = 1, FirstName = "Valon", LastName = "D", Email = "valon@est.local", IsActive = true, CreatedAt = DateTime.UtcNow });
        dbContext.AgentCompanies.Add(new AgentCompany { Id = 1, AgentId = 1, CompanyId = 1, IsActive = true, CreatedAt = DateTime.UtcNow });

        if (includeSecondAgentCompany)
        {
            dbContext.Agents.Add(new Agent { Id = 2, FirstName = "Dren", LastName = "K", Email = "dren@est.local", IsActive = true, CreatedAt = DateTime.UtcNow });
            dbContext.AgentCompanies.Add(new AgentCompany { Id = 2, AgentId = 2, CompanyId = 1, IsActive = true, CreatedAt = DateTime.UtcNow });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUserReferenceDataAsync(AppDbContext dbContext)
    {
        dbContext.Companies.Add(new Company { Id = 1, Name = "EstateIQ", IsActive = true, CreatedAt = DateTime.UtcNow });
        dbContext.Roles.AddRange(
            new Role { Id = Guid.NewGuid(), Name = "CompanyAdmin", CreatedAt = DateTime.UtcNow },
            new Role { Id = Guid.NewGuid(), Name = "Agent", CreatedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Property> SeedPropertyAsync(
        AppDbContext dbContext,
        int companyId = 1,
        int agentId = 1,
        int statusId = 1)
    {
        var property = new Property
        {
            Title = "Test Property",
            Price = 100_000m,
            Area = 80m,
            PropertyTypeId = 1,
            PropertyStatusId = statusId,
            CompanyId = companyId,
            AgentId = agentId,
            Address = "Main St",
            City = "Tirane",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();
        return property;
    }

    private static async Task<Guid> SeedCompanyAdminUserAsync(AppDbContext dbContext, int companyId)
    {
        var userId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = userId,
            FirstName = "Admin",
            LastName = "User",
            Email = "ca@example.com",
            PasswordHash = "hash",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        });
        dbContext.CompanyUsers.Add(new CompanyUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyId = companyId,
            RelationshipType = Roles.CompanyAdmin,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        return userId;
    }

    private static async Task<Guid> SeedAgentUserAsync(AppDbContext dbContext, int companyId)
    {
        var userId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = userId,
            FirstName = "Agent",
            LastName = "User",
            Email = "agentuser@example.com",
            PasswordHash = "hash",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        });
        var agent = new Agent
        {
            UserId = userId,
            FirstName = "Agent",
            LastName = "User",
            Email = "agentuser@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Agents.Add(agent);
        await dbContext.SaveChangesAsync();

        dbContext.AgentCompanies.Add(new AgentCompany
        {
            AgentId = agent.Id,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        return userId;
    }

    private static async Task<Guid> SeedRegularUserAsync(AppDbContext dbContext)
    {
        var userId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = userId,
            FirstName = "Regular",
            LastName = "User",
            Email = "regular@example.com",
            PasswordHash = "hash",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        return userId;
    }

    private static CreatePropertyDto BuildCreatePropertyDto(int companyId, int agentId)
    {
        return new CreatePropertyDto
        {
            Title = "New Property",
            Description = "Desc",
            Price = 100_000m,
            Area = 80m,
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2020,
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            CompanyId = companyId,
            AgentId = agentId,
            Address = "Street 1",
            City = "Tirane",
            Latitude = 41.3m,
            Longitude = 19.8m
        };
    }

    private static UpdatePropertyDto BuildUpdatePropertyDto(Property property)
    {
        return new UpdatePropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description ?? "Desc",
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
