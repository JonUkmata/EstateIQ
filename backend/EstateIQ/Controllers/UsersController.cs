using EstateIQ.Constants;
using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.ManageUsers)]
public class UsersController(
    IUserService userService,
    ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet]
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
}
