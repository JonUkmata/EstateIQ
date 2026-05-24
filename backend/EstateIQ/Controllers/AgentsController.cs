using EstateIQ.Constants;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides public readonly agent lookup endpoints for frontend dropdowns.
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

    [HttpGet("my-company")]
    [Authorize(Policy = Permissions.ManageAgents)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<AgentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetMyCompanyAgents()
    {
        try
        {
            if (!User.IsInRole(Roles.CompanyAdmin))
            {
                return Forbid();
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var currentUserId))
            {
                return Forbid();
            }

            var agents = await _agentService.GetForCompanyAdminAsync(currentUserId);
            return Ok(agents);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching company agents for current CompanyAdmin.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching company agents.");
        }
    }
}
