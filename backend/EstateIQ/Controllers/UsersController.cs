using EstateIQ.Constants;
using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUserService userService,
    ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = Permissions.ManageUsers)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> GetUsers([FromQuery] UserListQueryParameters queryParameters)
    {
        try
        {
            var users = await _userService.GetUsersAsync(queryParameters);
            return Ok(users);
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching users.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching users.");
        }
    }

    [HttpPost("company-admins")]
    [Authorize(Policy = Permissions.ManageUsers)]
    [Authorize(Policy = Permissions.ManageCompanies)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateCompanyAdminResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateCompanyAdminResponseDto>> CreateCompanyAdmin([FromBody] CreateCompanyAdminRequestDto request)
    {
        try
        {
            var companyAdmin = await _userService.CreateCompanyAdminAsync(request);
            return Created($"/api/users/{companyAdmin.Id}", companyAdmin);
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
        catch (NotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating company admin.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating company admin.");
        }
    }

    [HttpPost("agents")]
    [Authorize(Policy = Permissions.ManageAgents)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateAgentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAgentResponseDto>> CreateAgent([FromBody] CreateAgentRequestDto request)
    {
        try
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var currentUserId))
            {
                return Forbid();
            }

            var isAdmin = User.IsInRole(Roles.Admin);
            var isCompanyAdmin = User.IsInRole(Roles.CompanyAdmin);
            if (!isAdmin && !isCompanyAdmin)
            {
                return Forbid();
            }

            var agent = await _userService.CreateAgentAsync(request, currentUserId, isAdmin);
            return Created($"/api/agents/{agent.AgentId}", agent);
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
        catch (NotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (BusinessRuleException exception)
        {
            if (string.Equals(exception.Message, "CompanyAdmin can only create agents for their own company.", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
            }

            return Conflict(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating agent user.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating agent.");
        }
    }
}
