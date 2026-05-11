namespace EstateIQ.DTOs.Users;

public class CreateAgentResponseDto
{
    public Guid UserId { get; set; }

    public int AgentId { get; set; }

    public string Email { get; set; } = string.Empty;

    public int CompanyId { get; set; }
}
