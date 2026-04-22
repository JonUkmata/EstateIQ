namespace EstateIQ.Models;

public class Property
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal Area { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? Floors { get; set; }

    public int? YearBuilt { get; set; }

    public int PropertyTypeId { get; set; }

    public int PropertyStatusId { get; set; }

    public int CompanyId { get; set; }

    public int AgentId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public PropertyType PropertyType { get; set; } = null!;

    public PropertyStatus PropertyStatus { get; set; } = null!;

    public Company Company { get; set; } = null!;

    public Agent Agent { get; set; } = null!;
}
