namespace EstateIQ.DTOs;

/// <summary>
/// Represents company data exposed by the API layer.
/// </summary>
public class CompanyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? City { get; set; }

    public bool IsActive { get; set; }
}
