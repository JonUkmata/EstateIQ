namespace EstateIQ.DTOs;

/// <summary>
/// Represents property type data exposed by the API layer.
/// </summary>
public class PropertyTypeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
