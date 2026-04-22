namespace EstateIQ.Models;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<AgentCompany> AgentCompanies { get; set; } = new List<AgentCompany>();

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
