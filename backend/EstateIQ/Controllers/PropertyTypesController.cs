using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides property type lookup endpoints for frontend dropdowns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PropertyTypesController(
    IPropertyTypeService propertyTypeService,
    ILogger<PropertyTypesController> logger) : ControllerBase
{
    private readonly IPropertyTypeService _propertyTypeService = propertyTypeService;
    private readonly ILogger<PropertyTypesController> _logger = logger;

    /// <summary>
    /// Gets property types for dropdown menus.
    /// </summary>
    /// <param name="includeInactive">When true, inactive property types are included.</param>
    /// <param name="search">Optional property type name search filter.</param>
    /// <returns>A lightweight list of property types sorted by name.</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<PropertyTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PropertyTypeDto>>> GetPropertyTypes(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null)
    {
        try
        {
            var propertyTypes = await _propertyTypeService.GetForDropdownAsync(includeInactive, search);
            return Ok(propertyTypes);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching property types for dropdown.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching property types.");
        }
    }
}
