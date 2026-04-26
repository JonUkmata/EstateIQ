using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides agent lookup endpoints for frontend dropdowns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController(
    IAgentService agentService,
    ILogger<AgentsController> logger) : ControllerBase
{
    private readonly IAgentService _agentService = agentService;
    private readonly ILogger<AgentsController> _logger = logger;

    /// <summary>
    /// Gets agents for dropdown menus.
    /// </summary>
    /// <param name="includeInactive">When true, inactive agents are included.</param>
    /// <param name="search">Optional agent name or email search filter.</param>
    /// <param name="companyId">Optional company filter for assigned agents.</param>
    /// <returns>A lightweight list of agents sorted by name.</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<AgentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] int? companyId = null)
    {
        try
        {
            var agents = await _agentService.GetForDropdownAsync(includeInactive, search, companyId);
            return Ok(agents);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching agents for dropdown.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching agents.");
        }
    }
}
