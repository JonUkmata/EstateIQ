namespace EstateIQ.DTOs.Users;

public class CreateCompanyAdminRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int CompanyId { get; set; }
}
