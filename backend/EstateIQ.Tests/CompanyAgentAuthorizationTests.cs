using System.Net;
using System.Net.Http.Headers;
using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.Extensions;
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

public class CompanyAgentAuthorizationTests
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
    public async Task CompanyAndAgentLookupEndpoints_RemainPublic()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var companiesResponse = await client.GetAsync("/api/companies");
        var agentsResponse = await client.GetAsync("/api/agents");

        Assert.Equal(HttpStatusCode.OK, companiesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, agentsResponse.StatusCode);
    }

    [Fact]
    public async Task UserToken_CannotAccessCompanyOrAgentManagementPolicies()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Roles.User, Permissions.ViewProperties, Permissions.BookViewing);

        var manageCompaniesResponse = await client.GetAsync("/api/test/permissions/manage-companies");
        var manageAgentsResponse = await client.GetAsync("/api/test/permissions/manage-agents");

        Assert.Equal(HttpStatusCode.Forbidden, manageCompaniesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, manageAgentsResponse.StatusCode);
    }

    [Fact]
    public async Task AdminToken_CanAccessCompanyAndAgentManagementPolicies()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Roles.Admin, Permissions.ManageCompanies, Permissions.ManageAgents);

        var manageCompaniesResponse = await client.GetAsync("/api/test/permissions/manage-companies");
        var manageAgentsResponse = await client.GetAsync("/api/test/permissions/manage-agents");

        Assert.Equal(HttpStatusCode.OK, manageCompaniesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, manageAgentsResponse.StatusCode);
    }

    [Fact]
    public async Task CompanyAdminToken_CanAccessAgentManagementPolicyOnly()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Roles.CompanyAdmin, Permissions.ManageAgents);

        var manageCompaniesResponse = await client.GetAsync("/api/test/permissions/manage-companies");
        var manageAgentsResponse = await client.GetAsync("/api/test/permissions/manage-agents");

        Assert.Equal(HttpStatusCode.Forbidden, manageCompaniesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, manageAgentsResponse.StatusCode);
    }

    [Fact]
    public void CompanyAdminOwnershipHelper_AllowsOnlyAssignedCompany()
    {
        var assignedCompanyPrincipal = ClaimsPrincipalFactory.Create(
            Roles.CompanyAdmin,
            companyId: "7");
        var otherCompanyPrincipal = ClaimsPrincipalFactory.Create(
            Roles.CompanyAdmin,
            companyId: "8");
        var adminPrincipal = ClaimsPrincipalFactory.Create(Roles.Admin);

        Assert.True(assignedCompanyPrincipal.CanManageAgentsForCompany(7));
        Assert.False(otherCompanyPrincipal.CanManageAgentsForCompany(7));
        Assert.True(adminPrincipal.CanManageAgentsForCompany(7));
    }

    private static void AddBearerToken(HttpClient client, string role, params string[] permissions)
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));
        var accessToken = tokenService.GenerateAccessToken(
            new User
            {
                Id = Guid.NewGuid(),
                Email = "company-agent-auth-test@example.com"
            },
            [role],
            permissions);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
