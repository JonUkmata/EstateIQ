using System.ComponentModel.DataAnnotations;

namespace EstateIQ.DTOs;

/// <summary>
/// Represents a lightweight company response for dropdown controls.
/// </summary>
public class CompanyDropdownDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? City { get; set; }

    public bool IsActive { get; set; }
}
