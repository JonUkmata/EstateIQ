using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;

namespace EstateIQ.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListQueryParameters queryParameters);
}
