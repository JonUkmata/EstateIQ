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

    [HttpPost("verify-email")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VerifyEmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VerifyEmailResponseDto>> VerifyEmail(VerifyEmailRequestDto request)
    {
        try
        {
            var response = await _authService.VerifyEmailAsync(request);
            return Ok(response);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Email verification failed"
            });
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Email verification failed",
                Detail = exception.Message
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error during email verification.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during email verification.");
        }
    }

    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            AppendRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

            return Ok(response);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Login failed"
            });
        }
        catch (BusinessRuleException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Login failed",
                Detail = exception.Message
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error during login.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during login.");
        }
    }

    private void AppendRefreshTokenCookie(string refreshToken, DateTime refreshTokenExpiresAt)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = refreshTokenExpiresAt
        });
    }
}
