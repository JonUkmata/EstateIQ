namespace EstateIQ.DTOs;

/// <summary>
/// Represents a generated property price suggestion and model-awareness warnings.
/// </summary>
public class GeneratePropertyPriceResponseDto
{
    public decimal SuggestedPrice { get; set; }

    public string FormattedPrice { get; set; } = string.Empty;

    public IReadOnlyList<string> MlWarnings { get; set; } = [];
}
