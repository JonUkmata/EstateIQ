using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides property management endpoints.
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
    /// Gets properties with optional filtering and pagination.
    /// </summary>
    /// <param name="queryParameters">The filter and pagination query parameters.</param>
    /// <returns>A filtered and paginated list of properties with related lookup data.</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<PropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<PropertyDto>>> GetProperties([FromQuery] PropertyQueryParameters queryParameters)
    {
        try
        {
            var properties = await _propertyService.GetFilteredAsync(queryParameters);
            return Ok(properties);
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching properties.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching properties.");
        }
    }

    /// <summary>
    /// Gets a property by identifier.
    /// </summary>
    /// <param name="id">The property identifier.</param>
    /// <returns>The requested property with related lookup data.</returns>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PropertyDto>> GetProperty(int id)
    {
        try
        {
            var property = await _propertyService.GetByIdAsync(id);
            return Ok(property);
        }
        catch (NotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching property {PropertyId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the property.");
        }
    }

    /// <summary>
    /// Creates a property.
    /// </summary>
    /// <param name="dto">The property creation payload.</param>
    /// <returns>The created property with related lookup data.</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PropertyDto>> CreateProperty([FromBody] CreatePropertyDto dto)
    {
        try
        {
            var property = await _propertyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, property);
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating property.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the property.");
        }
    }

    /// <summary>
    /// Deletes a property when business rules allow it.
    /// </summary>
    /// <param name="id">The property identifier.</param>
    /// <returns>No content when the property is deleted.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        try
        {
            var deleted = await _propertyService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound($"Property with id {id} was not found.");
            }

            return NoContent();
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error deleting property {PropertyId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the property.");
        }
    }
}
