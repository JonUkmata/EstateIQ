using EstateIQ.Extensions;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(
    IDashboardService dashboardService,
    ILogger<DashboardController> logger) : ControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;
    private readonly ILogger<DashboardController> _logger = logger;

    /// <summary>
    /// Returns a dashboard summary tailored to the current user's role.
    /// Role precedence: Admin > CompanyAdmin > Agent > User.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyDashboard()
    {
        var userId = User.GetUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _dashboardService.GetDashboardAsync(userId.Value, User);
            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching dashboard for user {UserId}.", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the dashboard.");
        }
    }
}
