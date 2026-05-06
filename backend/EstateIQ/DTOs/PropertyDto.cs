using EstateIQ.DTOs.Files;

namespace EstateIQ.DTOs;

/// <summary>
/// Represents property data returned by the service layer.
/// </summary>
public class PropertyDto
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

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public PropertyTypeDto PropertyType { get; set; } = null!;

    public PropertyStatusDto PropertyStatus { get; set; } = null!;

    public CompanyDto Company { get; set; } = null!;

    public AgentDto Agent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<FileResponseDto> Images { get; set; } = [];
}
