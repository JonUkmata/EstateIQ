namespace EstateIQ.DTOs;

/// <summary>
/// Represents property status data exposed by the API layer.
/// </summary>
public class PropertyStatusDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ColorCode { get; set; }
}
