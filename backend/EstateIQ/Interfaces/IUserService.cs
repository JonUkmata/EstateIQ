using EstateIQ.DTOs;
using EstateIQ.DTOs.Users;

namespace EstateIQ.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListQueryParameters queryParameters);

    Task<CreateCompanyAdminResponseDto> CreateCompanyAdminAsync(CreateCompanyAdminRequestDto request);

    Task<CreateAgentResponseDto> CreateAgentAsync(CreateAgentRequestDto request, Guid currentUserId, bool isAdmin);

    Task<UpdateUserStatusResponseDto> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequestDto request);
}
