using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class AgentCompanySeederTests
{
    [Fact]
    public async Task SeedRequiredAgentsAndRelationshipsAsync_CreatesAgentsAndRelationships()
    {
        await using var dbContext = CreateContext();
        await SeedCompaniesAsync(dbContext);

        await AgentCompanySeeder.SeedRequiredAgentsAndRelationshipsAsync(dbContext);

        var seededEmails = new[]
        {
            "ardit.hoxha@estateiq.local",
            "lea.krasniqi@estateiq.local",
            "mira.berisha@estateiq.local",
            "dren.gashi@estateiq.local",
            "nora.shala@estateiq.local"
        };

        var agents = await dbContext.Agents
            .Where(agent => seededEmails.Contains(agent.Email))
            .ToListAsync();

        Assert.True(agents.Count >= 5);

        var agentIds = agents.Select(agent => agent.Id).ToHashSet();
        var relationships = await dbContext.AgentCompanies
            .Where(relation => agentIds.Contains(relation.AgentId))
            .ToListAsync();

        Assert.NotEmpty(relationships);
        Assert.All(agentIds, agentId =>
            Assert.Contains(relationships, relation => relation.AgentId == agentId));
    }

    [Fact]
    public async Task SeedRequiredAgentsAndRelationshipsAsync_DoesNotCreateDuplicates()
    {
        await using var dbContext = CreateContext();
        await SeedCompaniesAsync(dbContext);

        await AgentCompanySeeder.SeedRequiredAgentsAndRelationshipsAsync(dbContext);
        await AgentCompanySeeder.SeedRequiredAgentsAndRelationshipsAsync(dbContext);

        var duplicateAgentEmails = await dbContext.Agents
            .GroupBy(agent => agent.Email)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToListAsync();

        var duplicateRelationships = await dbContext.AgentCompanies
            .GroupBy(relation => new { relation.AgentId, relation.CompanyId })
            .Where(group => group.Count() > 1)
            .Select(group => new { group.Key.AgentId, group.Key.CompanyId })
            .ToListAsync();

        Assert.Empty(duplicateAgentEmails);
        Assert.Empty(duplicateRelationships);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedCompaniesAsync(AppDbContext dbContext)
    {
        dbContext.Companies.AddRange(
            new Company
            {
                Name = "Company A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Company
            {
                Name = "Company B",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();
    }
}
