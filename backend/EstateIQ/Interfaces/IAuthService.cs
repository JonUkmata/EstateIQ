using EstateIQ.DTOs.Auth;

namespace EstateIQ.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
}
