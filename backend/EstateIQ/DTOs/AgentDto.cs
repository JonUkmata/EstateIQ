namespace EstateIQ.DTOs;

/// <summary>
/// Represents agent data exposed by the API layer.
/// </summary>
public class AgentDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public bool IsActive { get; set; }
}
