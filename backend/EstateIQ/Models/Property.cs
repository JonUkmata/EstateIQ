using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateIQ.Models;

[Table("Properties")]
public class Property
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal Area { get; set; }

    public int? Bedrooms { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? Bathrooms { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? Floors { get; set; }

    [Range(1800, int.MaxValue)]
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
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    public int? Zipcode { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? LotArea { get; set; }

    [StringLength(10)]
    public string? LotAreaUnit { get; set; }

    [Range(1, 5)]
    public int? Condition { get; set; }

    [Range(1, 13)]
    public int? Grade { get; set; }

    public bool? HasBasement { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? BasementArea { get; set; }

    [StringLength(10)]
    public string? BasementAreaUnit { get; set; }

    public bool? Waterfront { get; set; }

    [Range(0, 4)]
    public int? ViewQuality { get; set; }

    public bool? Renovated { get; set; }

    [Range(1800, int.MaxValue)]
    public int? YearRenovated { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? NearbyLivingArea { get; set; }

    [StringLength(10)]
    public string? NearbyLivingAreaUnit { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? NearbyLotArea { get; set; }

    [StringLength(10)]
    public string? NearbyLotAreaUnit { get; set; }

    [Column(TypeName = "decimal(10,8)")]
    [Range(typeof(decimal), "-90", "90")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    [Range(typeof(decimal), "-180", "180")]
    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(PropertyTypeId))]
    public PropertyType PropertyType { get; set; } = null!;

    [ForeignKey(nameof(PropertyStatusId))]
    public PropertyStatus PropertyStatus { get; set; } = null!;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(AgentId))]
    public Agent Agent { get; set; } = null!;
}
