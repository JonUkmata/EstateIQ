namespace EstateIQ.DTOs.Auth;

public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;

    public bool VerificationEmailSent { get; set; }

    public string? VerificationToken { get; set; }
}
