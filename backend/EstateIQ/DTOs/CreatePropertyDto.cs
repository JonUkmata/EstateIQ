using System.ComponentModel.DataAnnotations;

namespace EstateIQ.DTOs;

/// <summary>
/// Represents the payload required to create a property.
/// </summary>
public class CreatePropertyDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Area { get; set; }

    [Range(0, 100)]
    public int? Bedrooms { get; set; }

    [Range(0, 50)]
    public decimal? Bathrooms { get; set; }

    [Range(0, 200)]
    public decimal? Floors { get; set; }

    [Range(1800, 2100)]
    public int? YearBuilt { get; set; }

    [Required]
    public int PropertyTypeId { get; set; }

    [Required]
    public int PropertyStatusId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int AgentId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(1, 99999)]
    public int? Zipcode { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? LotArea { get; set; }

    [MaxLength(10)]
    public string? LotAreaUnit { get; set; }

    [Range(1, 5)]
    public int? Condition { get; set; }

    [Range(1, 13)]
    public int? Grade { get; set; }

    public bool? HasBasement { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? BasementArea { get; set; }

    [MaxLength(10)]
    public string? BasementAreaUnit { get; set; }

    public bool? Waterfront { get; set; }

    [Range(0, 4)]
    public int? ViewQuality { get; set; }

    public bool? Renovated { get; set; }

    [Range(1800, 2100)]
    public int? YearRenovated { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? NearbyLivingArea { get; set; }

    [MaxLength(10)]
    public string? NearbyLivingAreaUnit { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? NearbyLotArea { get; set; }

    [MaxLength(10)]
    public string? NearbyLotAreaUnit { get; set; }

    [Range(typeof(decimal), "-90", "90")]
    public decimal? Latitude { get; set; }

    [Range(typeof(decimal), "-180", "180")]
    public decimal? Longitude { get; set; }
}
