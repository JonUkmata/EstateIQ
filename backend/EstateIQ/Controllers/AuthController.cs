using EstateIQ.DTOs.Auth;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstateIQ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Created("/api/auth/register", response);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            });
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration failed",
                Detail = exception.Message
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error during public user registration.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during registration.");
        }
    }
}
