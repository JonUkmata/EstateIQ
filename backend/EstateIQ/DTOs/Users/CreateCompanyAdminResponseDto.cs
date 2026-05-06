namespace EstateIQ.DTOs.Users;

public class CreateCompanyAdminResponseDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int CompanyId { get; set; }
}
