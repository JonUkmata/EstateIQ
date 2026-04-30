using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides property detail endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PropertiesController(
    IPropertyService propertyService,
    ILogger<PropertiesController> logger) : ControllerBase
{
    private readonly IPropertyService _propertyService = propertyService;
    private readonly ILogger<PropertiesController> _logger = logger;

    /// <summary>
    /// Gets a single property with full details by identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PropertyDto>> GetById(int id)
    {
        try
        {
            var property = await _propertyService.GetByIdAsync(id);
            return Ok(property);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching property details for id {PropertyId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching property details.");
        }
    }
}
