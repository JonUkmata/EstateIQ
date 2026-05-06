namespace EstateIQ.Models;

public class Agent
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }

    public ICollection<AgentCompany> AgentCompanies { get; set; } = new List<AgentCompany>();

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
