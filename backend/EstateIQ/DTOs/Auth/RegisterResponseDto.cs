namespace EstateIQ.DTOs.Auth;

public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string VerificationToken { get; set; } = string.Empty;
}
