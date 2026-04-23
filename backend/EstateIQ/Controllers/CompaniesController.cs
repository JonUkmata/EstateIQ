using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

/// <summary>
/// Provides company lookup endpoints for frontend dropdowns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CompaniesController(
    ICompanyService companyService,
    ILogger<CompaniesController> logger) : ControllerBase
{
    private readonly ICompanyService _companyService = companyService;
    private readonly ILogger<CompaniesController> _logger = logger;

    /// <summary>
    /// Gets companies for dropdown menus.
    /// </summary>
    /// <param name="includeInactive">When true, inactive companies are included.</param>
    /// <param name="search">Optional company name search filter.</param>
    /// <returns>A lightweight list of companies sorted by name.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/companies
    ///     GET /api/companies?includeInactive=true
    ///     GET /api/companies?search=ABC
    ///
    /// </remarks>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CompanyDropdownDto>>> GetCompanies(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null)
    {
        try
        {
            var companies = await _companyService.GetForDropdownAsync(includeInactive, search);
            return Ok(companies);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching companies for dropdown.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching companies.");
        }
    }
}
