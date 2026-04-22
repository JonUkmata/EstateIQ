namespace EstateIQ.Models;

public class AgentCompany
{
    public int Id { get; set; }

    public int AgentId { get; set; }

    public int CompanyId { get; set; }

    public string? Role { get; set; }

    public DateOnly? JoinedDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public Agent Agent { get; set; } = null!;

    public Company Company { get; set; } = null!;
}
