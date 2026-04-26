using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides property status lookup endpoints for frontend dropdowns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PropertyStatusesController(
    IPropertyStatusService propertyStatusService,
    ILogger<PropertyStatusesController> logger) : ControllerBase
{
    private readonly IPropertyStatusService _propertyStatusService = propertyStatusService;
    private readonly ILogger<PropertyStatusesController> _logger = logger;

    /// <summary>
    /// Gets property statuses for dropdown menus.
    /// </summary>
    /// <param name="includeInactive">When true, inactive statuses are included.</param>
    /// <param name="search">Optional status name search filter.</param>
    /// <returns>A list of property statuses sorted by name.</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<PropertyStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PropertyStatusDto>>> GetPropertyStatuses(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null)
    {
        try
        {
            var statuses = await _propertyStatusService.GetForDropdownAsync(includeInactive, search);
            return Ok(statuses);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching property statuses for dropdown.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching property statuses.");
        }
    }
}
