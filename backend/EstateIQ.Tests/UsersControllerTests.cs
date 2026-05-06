using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.DTOs.Auth;
using EstateIQ.DTOs.Users;
using EstateIQ.Models;
using EstateIQ.Services.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class UsersControllerTests
{
    private static readonly JwtSettings TestJwtSettings = new()
    {
        Issuer = "EstateIQ",
        Audience = "EstateIQ",
        Key = "EstateIQ-Development-Jwt-Key-Replace-In-Production-2026",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    [Fact]
    public async Task GetUsers_WithManageUsersPermission_ReturnsPaginatedUsers()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedUsersAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.GetAsync("/api/users?search=jon&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserListItemDto>>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        var user = result.Items.Single();
        Assert.Equal("Jon", user.FirstName);
        Assert.Equal("jon@example.com", user.Email);
        Assert.True(user.IsActive);
        Assert.True(user.IsEmailConfirmed);
        Assert.Equal(["User"], user.Roles);
    }

    [Fact]
    public async Task GetUsers_WithoutManageUsersPermission_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedUsersAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedUsersAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ResponseDoesNotContainPasswordHash()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedUsersAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hashed-password", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUsers_InvalidPagination_ReturnsBadRequest()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedUsersAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.GetAsync("/api/users?page=0&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCompanyAdmin_WithRequiredPermissions_CreatesUserRoleAndCompanyLink()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompanyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers, Permissions.ManageCompanies);

        var response = await client.PostAsJsonAsync("/api/users/company-admins", BuildCompanyAdminRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateCompanyAdminResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("companyadmin@example.com", result!.Email);
        Assert.Equal(Roles.CompanyAdmin, result.Role);
        Assert.Equal(1, result.CompanyId);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var userRole = await dbContext.UserRoles.Include(x => x.Role).SingleAsync();
        var companyUser = await dbContext.CompanyUsers.SingleAsync();

        Assert.True(user.IsActive);
        Assert.True(user.IsEmailConfirmed);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Equal(Roles.CompanyAdmin, userRole.Role.Name);
        Assert.Equal(1, companyUser.CompanyId);
        Assert.Equal(user.Id, companyUser.UserId);
        Assert.Equal(Roles.CompanyAdmin, companyUser.RelationshipType);
    }

    [Fact]
    public async Task CreateCompanyAdmin_WithoutManageCompaniesPermission_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompanyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.PostAsJsonAsync("/api/users/company-admins", BuildCompanyAdminRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCompanyAdmin_InvalidCompany_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompanyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers, Permissions.ManageCompanies);

        var request = BuildCompanyAdminRequest();
        request.CompanyId = 999;
        var response = await client.PostAsJsonAsync("/api/users/company-admins", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCompanyAdmin_DuplicateEmail_ReturnsConflict()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompanyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers, Permissions.ManageCompanies);
        await client.PostAsJsonAsync("/api/users/company-admins", BuildCompanyAdminRequest());

        var response = await client.PostAsJsonAsync("/api/users/company-admins", BuildCompanyAdminRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_AdminCanCreateAgentForAnyCompany()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompaniesAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Guid.NewGuid(), [Roles.Admin], Permissions.ManageAgents);

        var response = await client.PostAsJsonAsync("/api/users/agents", BuildAgentRequest(companyId: 2));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateAgentResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("agent@example.com", result!.Email);
        Assert.Equal(2, result.CompanyId);
        Assert.True(result.AgentId > 0);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var userRole = await dbContext.UserRoles.Include(x => x.Role).SingleAsync();
        var agent = await dbContext.Agents.SingleAsync();
        var agentCompany = await dbContext.AgentCompanies.SingleAsync();

        Assert.Equal(result.UserId, user.Id);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(user.IsEmailConfirmed);
        Assert.Equal(Roles.Agent, userRole.Role.Name);
        Assert.Equal(user.Id, agent.UserId);
        Assert.Equal(result.AgentId, agent.Id);
        Assert.Equal(2, agentCompany.CompanyId);
        Assert.Equal(agent.Id, agentCompany.AgentId);
    }

    [Fact]
    public async Task CreateAgent_CompanyAdminCanCreateAgentForOwnCompany()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var companyAdminUserId = await factory.SeedCompanyAdminAsync(companyId: 1);
        using var client = factory.CreateClient();
        AddBearerToken(client, companyAdminUserId, [Roles.CompanyAdmin], Permissions.ManageAgents);

        var response = await client.PostAsJsonAsync("/api/users/agents", BuildAgentRequest(companyId: 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_CompanyAdminCannotCreateAgentForOtherCompany()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var companyAdminUserId = await factory.SeedCompanyAdminAsync(companyId: 1);
        using var client = factory.CreateClient();
        AddBearerToken(client, companyAdminUserId, [Roles.CompanyAdmin], Permissions.ManageAgents);

        var response = await client.PostAsJsonAsync("/api/users/agents", BuildAgentRequest(companyId: 2));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_DuplicateEmail_ReturnsConflict()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedCompaniesAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Guid.NewGuid(), [Roles.Admin], Permissions.ManageAgents);
        await client.PostAsJsonAsync("/api/users/agents", BuildAgentRequest(companyId: 1));

        var response = await client.PostAsJsonAsync("/api/users/agents", BuildAgentRequest(companyId: 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_DeactivateUserBlocksLoginAndRefresh()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var userId = await factory.SeedLoginReadyUserAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.PatchAsJsonAsync($"/api/users/{userId}/status", new UpdateUserStatusRequestDto
        {
            IsActive = false
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UpdateUserStatusResponseDto>();
        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.False(result.IsActive);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedRefreshToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotNull(storedRefreshToken.RevokedAt);

        using var authClient = factory.CreateClient();
        var loginResponse = await authClient.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "login.ready@example.com",
            Password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);

        var refreshResponse = await authClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto
        {
            RefreshToken = "active-refresh-token"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithoutManageUsersPermission_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var userId = await factory.SeedLoginReadyUserAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties);

        var response = await client.PatchAsJsonAsync($"/api/users/{userId}/status", new UpdateUserStatusRequestDto
        {
            IsActive = false
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ReactivateUserAllowsLoginAgain()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var userId = await factory.SeedLoginReadyUserAsync(isActive: false);
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ManageUsers);

        var response = await client.PatchAsJsonAsync($"/api/users/{userId}/status", new UpdateUserStatusRequestDto
        {
            IsActive = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var authClient = factory.CreateClient();
        var loginResponse = await authClient.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "login.ready@example.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    private static void AddBearerToken(HttpClient client, params string[] permissions)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(permissions));
    }

    private static void AddBearerToken(HttpClient client, Guid userId, string[] roles, params string[] permissions)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(userId, roles, permissions));
    }

    private static string GenerateToken(params string[] permissions)
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));

        return tokenService.GenerateAccessToken(
            new User
            {
                Id = Guid.NewGuid(),
                Email = "users-test@example.com"
            },
            [Roles.Admin],
            permissions);
    }

    private static string GenerateToken(Guid userId, string[] roles, params string[] permissions)
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));

        return tokenService.GenerateAccessToken(
            new User
            {
                Id = userId,
                Email = "users-test@example.com"
            },
            roles,
            permissions);
    }

    private static CreateCompanyAdminRequestDto BuildCompanyAdminRequest()
    {
        return new CreateCompanyAdminRequestDto
        {
            FirstName = "Company",
            LastName = "Admin",
            Email = "companyadmin@example.com",
            Password = "Password123!",
            CompanyId = 1
        };
    }

    private static CreateAgentRequestDto BuildAgentRequest(int companyId)
    {
        return new CreateAgentRequestDto
        {
            FirstName = "Agent",
            LastName = "One",
            Email = "agent@example.com",
            Password = "Password123!",
            Phone = "+38344111222",
            CompanyId = companyId
        };
    }

    private sealed class EstateIqWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EstateIQTests;Trusted_Connection=True;TrustServerCertificate=True",
                    ["Redis:ConnectionString"] = "localhost:6379",
                    ["Jwt:Issuer"] = TestJwtSettings.Issuer,
                    ["Jwt:Audience"] = TestJwtSettings.Audience,
                    ["Jwt:Key"] = TestJwtSettings.Key,
                    ["Jwt:AccessTokenMinutes"] = TestJwtSettings.AccessTokenMinutes.ToString(),
                    ["Jwt:RefreshTokenDays"] = TestJwtSettings.RefreshTokenDays.ToString()
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }

        public async Task SeedUsersAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var userRole = await dbContext.Roles.SingleAsync(role => role.Name == Roles.User);
            var adminRole = await dbContext.Roles.SingleAsync(role => role.Name == Roles.Admin);

            var jon = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jon",
                LastName = "Ukmata",
                Email = "jon@example.com",
                PasswordHash = "hashed-password-jon",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PasswordHash = "hashed-password-admin",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.AddRange(jon, admin);
            dbContext.UserRoles.AddRange(
                new UserRole { Id = Guid.NewGuid(), UserId = jon.Id, RoleId = userRole.Id, AssignedAt = DateTime.UtcNow },
                new UserRole { Id = Guid.NewGuid(), UserId = admin.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow });

            await dbContext.SaveChangesAsync();
        }

        public async Task SeedCompanyAsync()
        {
            await SeedCompaniesAsync(companyCount: 1);
        }

        public async Task SeedCompaniesAsync(int companyCount = 2)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            for (var id = 1; id <= companyCount; id++)
            {
                dbContext.Companies.Add(new Company
                {
                    Id = id,
                    Name = $"Company {id}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<Guid> SeedCompanyAdminAsync(int companyId)
        {
            await SeedCompaniesAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var companyAdminRole = await dbContext.Roles.SingleAsync(role => role.Name == Roles.CompanyAdmin);
            var userId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FirstName = "Company",
                LastName = "Admin",
                Email = "company.admin@example.com",
                PasswordHash = "hash",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            });
            dbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = companyAdminRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            dbContext.CompanyUsers.Add(new CompanyUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                UserId = userId,
                RelationshipType = Roles.CompanyAdmin,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return userId;
        }

        public async Task<Guid> SeedLoginReadyUserAsync(bool isActive = true)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var passwordService = new PasswordService();
            var userRole = await dbContext.Roles.SingleAsync(role => role.Name == Roles.User);
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Login",
                LastName = "Ready",
                Email = "login.ready@example.com",
                IsActive = isActive,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = passwordService.HashPassword(user, "Password123!");

            dbContext.Users.Add(user);
            dbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = userRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            dbContext.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = new TokenService(Options.Create(TestJwtSettings)).HashToken("active-refresh-token"),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return user.Id;
        }
    }
}
