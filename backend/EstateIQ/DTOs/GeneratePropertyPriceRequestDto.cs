namespace EstateIQ.DTOs;

/// <summary>
/// Represents the agent-facing property details used to request an ML price estimate.
/// </summary>
public class GeneratePropertyPriceRequestDto
{
    public decimal LivingArea { get; set; }

    public string LivingAreaUnit { get; set; } = "m2";

    public int Bedrooms { get; set; }

    public decimal Bathrooms { get; set; }

    public decimal Floors { get; set; }

    public int YearBuilt { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Zipcode { get; set; }

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
}
