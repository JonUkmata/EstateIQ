using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;

namespace EstateIQ.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

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
}
