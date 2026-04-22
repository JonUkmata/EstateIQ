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
    public int? Bathrooms { get; set; }

    [Range(0, 200)]
    public int? Floors { get; set; }

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

    [Range(typeof(decimal), "-90", "90")]
    public decimal? Latitude { get; set; }

    [Range(typeof(decimal), "-180", "180")]
    public decimal? Longitude { get; set; }
}
