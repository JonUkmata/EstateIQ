using System.Net.Mail;
using EstateIQ.DTOs.Auth;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using EstateIQ.Models;

namespace EstateIQ.Services.Auth;

public class AuthService(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    private const string PublicUserRoleName = "User";
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        ValidateRegisterRequest(request);

        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _authRepository.EmailExistsAsync(normalizedEmail))
        {
            throw new BusinessRuleException("Email is already registered.");
        }

        var userRole = await _authRepository.GetRoleByNameAsync(PublicUserRoleName)
            ?? throw new BusinessRuleException("Public user role is not configured.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAt = now
        };

        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        var verificationToken = _tokenService.GenerateVerificationToken();
        var emailVerificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = verificationToken,
            ExpiresAt = now.AddDays(1),
            CreatedAt = now
        };

        var userRoleAssignment = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = userRole.Id,
            AssignedAt = now
        };

        await _authRepository.AddRegistrationAsync(user, userRoleAssignment, emailVerificationToken);

        _logger.LogInformation(
            "Registered public user {UserId} with email {Email}. Development verification token: {VerificationToken}",
            user.Id,
            user.Email,
            verificationToken);

        return new RegisterResponseDto
        {
            Message = "Registration successful. Please verify your email before logging in.",
            VerificationToken = verificationToken
        };
    }

    private static void ValidateRegisterRequest(RegisterRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        AddRequiredStringError(errors, nameof(request.FirstName), request.FirstName, 100);
        AddRequiredStringError(errors, nameof(request.LastName), request.LastName, 100);
        AddRequiredStringError(errors, nameof(request.Email), request.Email, 255);

        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email.Trim()))
        {
            errors[nameof(request.Email)] = ["Email must be a valid email address."];
        }

        var passwordErrors = GetPasswordErrors(request.Password).ToArray();
        if (passwordErrors.Length > 0)
        {
            errors[nameof(request.Password)] = passwordErrors;
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            errors[nameof(request.ConfirmPassword)] = ["ConfirmPassword must match Password."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void AddRequiredStringError(
        Dictionary<string, string[]> errors,
        string fieldName,
        string value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = [$"{fieldName} is required."];
            return;
        }

        if (value.Trim().Length > maxLength)
        {
            errors[fieldName] = [$"{fieldName} must be {maxLength} characters or fewer."];
        }
    }

    private static IEnumerable<string> GetPasswordErrors(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            yield return "Password is required.";
            yield break;
        }

        if (password.Length < 8)
        {
            yield return "Password must be at least 8 characters.";
        }

        if (!password.Any(char.IsUpper))
        {
            yield return "Password must include an uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            yield return "Password must include a lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            yield return "Password must include a number.";
        }

        if (!password.Any(character => !char.IsLetterOrDigit(character)))
        {
            yield return "Password must include a symbol.";
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
