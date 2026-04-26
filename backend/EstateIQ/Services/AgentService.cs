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
        string? search = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        _logger.LogInformation(
            "Fetching agents for dropdown. IncludeInactive: {IncludeInactive}, Search: {Search}",
            includeInactive,
            normalizedSearch ?? "None");

        IEnumerable<Agent> agents;

        if (normalizedSearch is not null)
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
