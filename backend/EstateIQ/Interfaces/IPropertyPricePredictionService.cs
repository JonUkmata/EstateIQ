using EstateIQ.DTOs;

namespace EstateIQ.Interfaces;

/// <summary>
/// Defines operations for generating suggested property prices.
/// </summary>
public interface IPropertyPricePredictionService
{
    /// <summary>
    /// Generates a suggested price from agent-facing property details.
    /// </summary>
    Task<GeneratePropertyPriceResponseDto> GenerateAsync(GeneratePropertyPriceRequestDto request, CancellationToken cancellationToken = default);
}
