using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using System.Net.Mail;

namespace EstateIQ.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordService passwordService) : IUserService
{
    private const string CompanyAdminRoleName = "CompanyAdmin";
    private const string CompanyAdminRelationshipType = "CompanyAdmin";
    private const string AgentRoleName = "Agent";
    private const string AgentCompanyRoleName = "Agent";
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListQueryParameters queryParameters)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);
        ValidateQueryParameters(queryParameters);

        var normalizedSearch = string.IsNullOrWhiteSpace(queryParameters.Search)
            ? null
            : queryParameters.Search.Trim();

        var (items, totalCount) = await _userRepository.GetPagedAsync(
            normalizedSearch,
            queryParameters.Page,
            queryParameters.PageSize);

        return new PagedResult<UserListItemDto>
        {
            Items = items.Select(user => new UserListItemDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.IsEmailConfirmed,
                Roles = user.UserRoles
                    .Select(userRole => userRole.Role.Name)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            }).ToList(),
            TotalCount = totalCount,
            Page = queryParameters.Page,
            PageSize = queryParameters.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize)
        };
    }

    public async Task<CreateCompanyAdminResponseDto> CreateCompanyAdminAsync(CreateCompanyAdminRequestDto request)
    {
        ValidateCreateCompanyAdminRequest(request);

        if (!await _userRepository.CompanyExistsAsync(request.CompanyId))
        {
            throw new NotFoundException($"Company with id {request.CompanyId} was not found.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
        {
            throw new BusinessRuleException("Email is already registered.");
        }

        var role = await _userRepository.GetRoleByNameAsync(CompanyAdminRoleName)
            ?? throw new BusinessRuleException("CompanyAdmin role is not configured.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = now
        };
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = now
        };

        var companyUser = new CompanyUser
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            UserId = user.Id,
            RelationshipType = CompanyAdminRelationshipType,
            CreatedAt = now
        };

        await _userRepository.AddCompanyAdminAsync(user, userRole, companyUser);

        return new CreateCompanyAdminResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = CompanyAdminRoleName,
            CompanyId = request.CompanyId
        };
    }

    public async Task<CreateAgentResponseDto> CreateAgentAsync(CreateAgentRequestDto request, Guid currentUserId, bool isAdmin)
    {
        ValidateCreateAgentRequest(request);

        if (!await _userRepository.CompanyExistsAsync(request.CompanyId))
        {
            throw new NotFoundException($"Company with id {request.CompanyId} was not found.");
        }

        if (!isAdmin &&
            !await _userRepository.CompanyUserExistsAsync(currentUserId, request.CompanyId, CompanyAdminRelationshipType))
        {
            throw new BusinessRuleException("CompanyAdmin can only create agents for their own company.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
        {
            throw new BusinessRuleException("Email is already registered.");
        }

        var role = await _userRepository.GetRoleByNameAsync(AgentRoleName)
            ?? throw new BusinessRuleException("Agent role is not configured.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = now
        };
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = now
        };

        var agent = new Agent
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            IsActive = true,
            CreatedAt = now
        };

        var agentCompany = new AgentCompany
        {
            Agent = agent,
            CompanyId = request.CompanyId,
            Role = AgentCompanyRoleName,
            IsActive = true,
            CreatedAt = now
        };

        await _userRepository.AddAgentUserAsync(user, userRole, agent, agentCompany);

        return new CreateAgentResponseDto
        {
            UserId = user.Id,
            AgentId = agent.Id,
            Email = user.Email,
            CompanyId = request.CompanyId
        };
    }

    public async Task<UpdateUserStatusResponseDto> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequestDto request)
    {
        ValidateUpdateUserStatusRequest(request);

        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        var newStatus = request.IsActive!.Value;
        user.IsActive = newStatus;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserStatusAsync(user, revokeRefreshTokens: !newStatus);

        return new UpdateUserStatusResponseDto
        {
            Id = user.Id,
            IsActive = user.IsActive
        };
    }

    private static void ValidateQueryParameters(UserListQueryParameters queryParameters)
    {
        var errors = new Dictionary<string, string[]>();

        if (queryParameters.Page <= 0)
        {
            errors[nameof(UserListQueryParameters.Page)] = ["Page must be greater than zero."];
        }

        if (queryParameters.PageSize <= 0)
        {
            errors[nameof(UserListQueryParameters.PageSize)] = ["PageSize must be greater than zero."];
        }
        else if (queryParameters.PageSize > 100)
        {
            errors[nameof(UserListQueryParameters.PageSize)] = ["PageSize must be less than or equal to 100."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void ValidateCreateCompanyAdminRequest(CreateCompanyAdminRequestDto request)
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

        if (request.CompanyId <= 0)
        {
            errors[nameof(request.CompanyId)] = ["CompanyId is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void ValidateCreateAgentRequest(CreateAgentRequestDto request)
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

        if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone.Trim().Length > 50)
        {
            errors[nameof(request.Phone)] = ["Phone must be 50 characters or fewer."];
        }

        if (request.CompanyId <= 0)
        {
            errors[nameof(request.CompanyId)] = ["CompanyId is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void ValidateUpdateUserStatusRequest(UpdateUserStatusRequestDto request)
    {
        if (request.IsActive is null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.IsActive)] = ["IsActive is required."]
            });
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
