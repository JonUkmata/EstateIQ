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

    public decimal? Bathrooms { get; set; }

    public decimal? Floors { get; set; }

    public int? YearBuilt { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int? Zipcode { get; set; }

    public decimal? LotArea { get; set; }

    public string? LotAreaUnit { get; set; }

    public int? Condition { get; set; }

    public int? Grade { get; set; }

    public bool? HasBasement { get; set; }

    public decimal? BasementArea { get; set; }

    public string? BasementAreaUnit { get; set; }

    public bool? Waterfront { get; set; }

    public int? ViewQuality { get; set; }

    public bool? Renovated { get; set; }

    public int? YearRenovated { get; set; }

    public decimal? NearbyLivingArea { get; set; }

    public string? NearbyLivingAreaUnit { get; set; }

    public decimal? NearbyLotArea { get; set; }

    public string? NearbyLotAreaUnit { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public PropertyTypeDto PropertyType { get; set; } = null!;

    public PropertyStatusDto PropertyStatus { get; set; } = null!;

    public CompanyDto Company { get; set; } = null!;

    public AgentDto Agent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<FileResponseDto> Images { get; set; } = [];

    public string? CoverImageUrl { get; set; }
}
