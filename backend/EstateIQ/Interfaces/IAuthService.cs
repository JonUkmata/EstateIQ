using EstateIQ.DTOs.Auth;

namespace EstateIQ.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<VerifyEmailResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request);

    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<RefreshTokenResponseDto> RefreshAsync(RefreshTokenRequestDto request);
}
