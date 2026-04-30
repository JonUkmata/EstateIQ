using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using EstateIQ.Models;

namespace EstateIQ.Services;

/// <summary>
/// Provides business logic operations for agent lookups.
/// </summary>
public class AgentService(
    IAgentRepository agentRepository,
    IMapper mapper,
    ILogger<AgentService> logger) : IAgentService
{
    private readonly IAgentRepository _agentRepository = agentRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AgentService> _logger = logger;

    /// <summary>
    /// Gets agents for dropdown usage.
    /// </summary>
    public async Task<IEnumerable<AgentDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null,
        int? companyId = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        _logger.LogInformation(
            "Fetching agents for dropdown. IncludeInactive: {IncludeInactive}, Search: {Search}, CompanyId: {CompanyId}",
            includeInactive,
            normalizedSearch ?? "None",
            companyId);

        IEnumerable<Agent> agents;

        if (companyId is > 0)
        {
            agents = await _agentRepository.GetActiveByCompanyAsync(companyId.Value);

            if (normalizedSearch is not null)
            {
                agents = agents.Where(agent =>
                    agent.FirstName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    agent.LastName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    agent.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
            }
        }
        else if (normalizedSearch is not null)
        {
            agents = await _agentRepository.SearchAsync(normalizedSearch);

            if (!includeInactive)
            {
                agents = agents.Where(agent => agent.IsActive);
            }
        }
        else if (includeInactive)
        {
            agents = await _agentRepository.GetAllAsync();
        }
        else
        {
            agents = await _agentRepository.GetAllActiveAsync();
        }

        var result = _mapper.Map<IEnumerable<AgentDto>>(agents).ToList();
        _logger.LogInformation("Returned {AgentCount} agents for dropdown.", result.Count);

        return result;
    }
}
