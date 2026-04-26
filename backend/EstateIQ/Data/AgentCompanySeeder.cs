using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public static class AgentCompanySeeder
{
    private static readonly (string FirstName, string LastName, string Email)[] RequiredAgents =
    [
        ("Ardit", "Hoxha", "ardit.hoxha@estateiq.local"),
        ("Lea", "Krasniqi", "lea.krasniqi@estateiq.local"),
        ("Mira", "Berisha", "mira.berisha@estateiq.local"),
        ("Dren", "Gashi", "dren.gashi@estateiq.local"),
        ("Nora", "Shala", "nora.shala@estateiq.local")
    ];

    public static async Task SeedRequiredAgentsAndRelationshipsAsync(AppDbContext dbContext)
    {
        var companyIds = await dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Id)
            .Select(company => company.Id)
            .ToListAsync();

        if (companyIds.Count == 0)
        {
            return;
        }

        var existingAgentsByEmail = await dbContext.Agents
            .AsNoTracking()
            .ToDictionaryAsync(agent => agent.Email, StringComparer.OrdinalIgnoreCase);

        var agentsToInsert = RequiredAgents
            .Where(requiredAgent => !existingAgentsByEmail.ContainsKey(requiredAgent.Email))
            .Select(requiredAgent => new Agent
            {
                FirstName = requiredAgent.FirstName,
                LastName = requiredAgent.LastName,
                Email = requiredAgent.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (agentsToInsert.Count > 0)
        {
            dbContext.Agents.AddRange(agentsToInsert);
            await dbContext.SaveChangesAsync();
        }

        var requiredAgentIdsByEmail = await dbContext.Agents
            .AsNoTracking()
            .Where(agent => RequiredAgents.Select(required => required.Email).Contains(agent.Email))
            .ToDictionaryAsync(agent => agent.Email, agent => agent.Id, StringComparer.OrdinalIgnoreCase);

        var existingRelationships = await dbContext.AgentCompanies
            .AsNoTracking()
            .Select(relation => new { relation.AgentId, relation.CompanyId })
            .ToListAsync();

        var relationshipSet = existingRelationships
            .Select(relation => (relation.AgentId, relation.CompanyId))
            .ToHashSet();

        var relationshipsToInsert = new List<AgentCompany>();

        for (var i = 0; i < RequiredAgents.Length; i++)
        {
            var requiredAgent = RequiredAgents[i];
            var agentId = requiredAgentIdsByEmail[requiredAgent.Email];
            var companyId = companyIds[i % companyIds.Count];

            if (relationshipSet.Contains((agentId, companyId)))
            {
                continue;
            }

            relationshipsToInsert.Add(new AgentCompany
            {
                AgentId = agentId,
                CompanyId = companyId,
                Role = "Agent",
                JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (relationshipsToInsert.Count == 0)
        {
            return;
        }

        dbContext.AgentCompanies.AddRange(relationshipsToInsert);
        await dbContext.SaveChangesAsync();
    }
}
