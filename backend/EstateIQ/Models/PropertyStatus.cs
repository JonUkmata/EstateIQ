namespace EstateIQ.Models;

public class PropertyStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ColorCode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
