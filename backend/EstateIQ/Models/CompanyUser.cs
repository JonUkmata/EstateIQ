namespace EstateIQ.Models;

public class CompanyUser
{
    public Guid Id { get; set; }

    public int CompanyId { get; set; }

    public Guid UserId { get; set; }

    public string RelationshipType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Company Company { get; set; } = null!;

    public User User { get; set; } = null!;
}
