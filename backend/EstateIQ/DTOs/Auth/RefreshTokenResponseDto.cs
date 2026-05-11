namespace EstateIQ.DTOs.Auth;

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
