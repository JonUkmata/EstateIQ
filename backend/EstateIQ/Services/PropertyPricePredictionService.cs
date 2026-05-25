using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;

namespace EstateIQ.Services;

/// <summary>
/// Maps listing-form details to the ML model contract and requests a price prediction.
/// </summary>
public class PropertyPricePredictionService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<PropertyPricePredictionService> logger) : IPropertyPricePredictionService
{
    private const decimal SquareMetersToSquareFeet = 10.7639m;
    private const int ModelSaleYear = 2014;
    private const int ModelSaleMonth = 11;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PropertyPricePredictionService> _logger = logger;

    public async Task<GeneratePropertyPriceResponseDto> GenerateAsync(
        GeneratePropertyPriceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        ValidateRequest(request, currentDate.Year);

        var mlRequest = BuildMlRequest(request, currentDate);
        var endpoint = _configuration["Ml:PredictUrl"] ?? "http://127.0.0.1:8000/predict";
        _logger.LogInformation(
            "ML price prediction request to {Endpoint}: {Payload}",
            endpoint,
            JsonSerializer.Serialize(mlRequest, JsonSerializerOptions.Web));

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, mlRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ML price prediction failed with status {StatusCode}.", response.StatusCode);
                throw new InvalidOperationException("The price prediction service is unavailable.");
            }

            var mlResponse = await response.Content.ReadFromJsonAsync<MlPricePredictionResponse>(cancellationToken);
            if (mlResponse is null)
            {
                throw new InvalidOperationException("The price prediction service returned an empty response.");
            }

            _logger.LogInformation(
                "ML price prediction response: {Payload}",
                JsonSerializer.Serialize(mlResponse, JsonSerializerOptions.Web));

            return new GeneratePropertyPriceResponseDto
            {
                SuggestedPrice = mlResponse.PredictedPrice,
                FormattedPrice = string.IsNullOrWhiteSpace(mlResponse.PredictedPriceFormatted)
                    ? FormatCurrency(mlResponse.PredictedPrice)
                    : mlResponse.PredictedPriceFormatted,
                MlWarnings = BuildWarnings(request)
            };
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(exception, "Unable to generate property price.");
            throw new InvalidOperationException("Unable to generate a price right now. Please try again later.", exception);
        }
    }

    private static MlPricePredictionRequest BuildMlRequest(GeneratePropertyPriceRequestDto request, DateTime currentDate)
    {
        var sqftLiving = ConvertToSquareFeet(request.LivingArea, request.LivingAreaUnit);
        var sqftLot = request.LotArea.HasValue
            ? ConvertToSquareFeet(request.LotArea.Value, request.LotAreaUnit ?? request.LivingAreaUnit)
            : sqftLiving;
        var sqftBasement = request.HasBasement == true && request.BasementArea.HasValue
            ? ConvertToSquareFeet(request.BasementArea.Value, request.BasementAreaUnit ?? request.LivingAreaUnit)
            : 0;
        var sqftAbove = sqftLiving - sqftBasement;
        var sqftLiving15 = request.NearbyLivingArea.HasValue
            ? ConvertToSquareFeet(request.NearbyLivingArea.Value, request.NearbyLivingAreaUnit ?? request.LivingAreaUnit)
            : sqftLiving;
        var sqftLot15 = request.NearbyLotArea.HasValue
            ? ConvertToSquareFeet(request.NearbyLotArea.Value, request.NearbyLotAreaUnit ?? request.LotAreaUnit ?? request.LivingAreaUnit)
            : sqftLot;

        return new MlPricePredictionRequest
        {
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            SqftLiving = sqftLiving,
            SqftLot = sqftLot,
            Floors = request.Floors,
            Waterfront = request.Waterfront == true ? 1 : 0,
            View = request.ViewQuality ?? 0,
            Condition = request.Condition ?? 3,
            Grade = request.Grade ?? 7,
            SqftAbove = sqftAbove,
            SqftBasement = sqftBasement,
            YearBuilt = request.YearBuilt,
            YearRenovated = request.Renovated == true ? request.YearRenovated ?? 0 : 0,
            Zipcode = request.Zipcode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SqftLiving15 = sqftLiving15,
            SqftLot15 = sqftLot15,
            SaleYear = ModelSaleYear,
            SaleMonth = ModelSaleMonth
        };
    }

    private static void ValidateRequest(GeneratePropertyPriceRequestDto request, int currentYear)
    {
        var errors = new Dictionary<string, string[]>();

        AddIf(request.LivingArea <= 0, errors, nameof(request.LivingArea), "Living Area must be greater than zero.");
        AddIf(!IsSupportedAreaUnit(request.LivingAreaUnit), errors, nameof(request.LivingAreaUnit), "Area Unit must be m2 or sqft.");
        AddIf(request.Bedrooms < 1 || request.Bedrooms > 10, errors, nameof(request.Bedrooms), "Bedrooms must be between 1 and 10.");
        AddIf(request.Bathrooms < 0.5m || request.Bathrooms > 8, errors, nameof(request.Bathrooms), "Bathrooms must be between 0.5 and 8.");
        AddIf(request.Floors <= 0 || request.Floors > 3.5m, errors, nameof(request.Floors), "Floors must be between 1 and 3.5.");
        AddIf(request.YearBuilt < 1800 || request.YearBuilt > currentYear + 2, errors, nameof(request.YearBuilt), $"Year Built must be between 1800 and {currentYear + 2}.");
        AddIf(request.Latitude < -90 || request.Latitude > 90, errors, nameof(request.Latitude), "Latitude must be between -90 and 90.");
        AddIf(request.Longitude < -180 || request.Longitude > 180, errors, nameof(request.Longitude), "Longitude must be between -180 and 180.");
        AddIf(request.Zipcode <= 0, errors, nameof(request.Zipcode), "Zipcode is required.");

        if (request.LotArea.HasValue)
        {
            AddIf(request.LotArea <= 0, errors, nameof(request.LotArea), "Lot Area must be greater than zero.");
            AddIf(!IsSupportedAreaUnit(request.LotAreaUnit ?? request.LivingAreaUnit), errors, nameof(request.LotAreaUnit), "Lot Area Unit must be m2 or sqft.");
        }

        if (request.Condition.HasValue)
        {
            AddIf(request.Condition < 1 || request.Condition > 5, errors, nameof(request.Condition), "Condition must be between 1 and 5.");
        }

        if (request.Grade.HasValue)
        {
            AddIf(request.Grade < 1 || request.Grade > 13, errors, nameof(request.Grade), "Quality Grade must be between 1 and 13.");
        }

        if (request.ViewQuality.HasValue)
        {
            AddIf(request.ViewQuality < 0 || request.ViewQuality > 4, errors, nameof(request.ViewQuality), "View Quality must be between 0 and 4.");
        }

        if (request.HasBasement == true && request.BasementArea.HasValue)
        {
            AddIf(request.BasementArea <= 0, errors, nameof(request.BasementArea), "Basement Area must be greater than zero.");
            AddIf(!IsSupportedAreaUnit(request.BasementAreaUnit ?? request.LivingAreaUnit), errors, nameof(request.BasementAreaUnit), "Basement Area Unit must be m2 or sqft.");

            var sqftLiving = ConvertToSquareFeet(request.LivingArea, request.LivingAreaUnit);
            var sqftBasement = ConvertToSquareFeet(request.BasementArea.Value, request.BasementAreaUnit ?? request.LivingAreaUnit);
            AddIf(sqftBasement > sqftLiving, errors, nameof(request.BasementArea), "Basement Area cannot be greater than Living Area.");
        }

        if (request.Renovated == true)
        {
            AddIf(!request.YearRenovated.HasValue, errors, nameof(request.YearRenovated), "Year Renovated is required when Renovated is selected.");
            if (request.YearRenovated.HasValue)
            {
                AddIf(request.YearRenovated < request.YearBuilt, errors, nameof(request.YearRenovated), "Year Renovated cannot be before Year Built.");
                AddIf(request.YearRenovated > currentYear, errors, nameof(request.YearRenovated), $"Year Renovated cannot be after {currentYear}.");
            }
        }

        if (request.NearbyLivingArea.HasValue)
        {
            AddIf(request.NearbyLivingArea <= 0, errors, nameof(request.NearbyLivingArea), "Nearby Living Area must be greater than zero.");
            AddIf(!IsSupportedAreaUnit(request.NearbyLivingAreaUnit ?? request.LivingAreaUnit), errors, nameof(request.NearbyLivingAreaUnit), "Nearby Living Area Unit must be m2 or sqft.");
        }

        if (request.NearbyLotArea.HasValue)
        {
            AddIf(request.NearbyLotArea <= 0, errors, nameof(request.NearbyLotArea), "Nearby Lot Area must be greater than zero.");
            AddIf(!IsSupportedAreaUnit(request.NearbyLotAreaUnit ?? request.LotAreaUnit ?? request.LivingAreaUnit), errors, nameof(request.NearbyLotAreaUnit), "Nearby Lot Area Unit must be m2 or sqft.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static IReadOnlyList<string> BuildWarnings(GeneratePropertyPriceRequestDto request)
    {
        var warnings = new List<string>();

        if (request.Latitude < 47.1559m || request.Latitude > 47.7776m ||
            request.Longitude < -122.519m || request.Longitude > -121.315m)
        {
            warnings.Add("Location is outside the model training area.");
        }

        if (request.Zipcode < 98001 || request.Zipcode > 98199)
        {
            warnings.Add("Zipcode is outside the model training area.");
        }

        if (request.YearBuilt < 1900 || request.YearBuilt > 2015)
        {
            warnings.Add("Year built is outside the model training range.");
        }

        return warnings;
    }

    private static int ConvertToSquareFeet(decimal value, string unit)
    {
        return IsSquareMeters(unit)
            ? (int)Math.Round(value * SquareMetersToSquareFeet, MidpointRounding.AwayFromZero)
            : (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static bool IsSupportedAreaUnit(string? unit) => IsSquareMeters(unit) || string.Equals(unit, "sqft", StringComparison.OrdinalIgnoreCase);

    private static bool IsSquareMeters(string? unit) =>
        string.Equals(unit, "m2", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(unit, "m²", StringComparison.OrdinalIgnoreCase);

    private static void AddIf(bool condition, Dictionary<string, string[]> errors, string key, string message)
    {
        if (condition)
        {
            errors[key] = [message];
        }
    }

    private static string FormatCurrency(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value:C0}");

    private sealed class MlPricePredictionRequest
    {
        [JsonPropertyName("bedrooms")]
        public int Bedrooms { get; init; }

        [JsonPropertyName("bathrooms")]
        public decimal Bathrooms { get; init; }

        [JsonPropertyName("sqft_living")]
        public int SqftLiving { get; init; }

        [JsonPropertyName("sqft_lot")]
        public int SqftLot { get; init; }

        [JsonPropertyName("floors")]
        public decimal Floors { get; init; }

        [JsonPropertyName("waterfront")]
        public int Waterfront { get; init; }

        [JsonPropertyName("view")]
        public int View { get; init; }

        [JsonPropertyName("condition")]
        public int Condition { get; init; }

        [JsonPropertyName("grade")]
        public int Grade { get; init; }

        [JsonPropertyName("sqft_above")]
        public int SqftAbove { get; init; }

        [JsonPropertyName("sqft_basement")]
        public int SqftBasement { get; init; }

        [JsonPropertyName("yr_built")]
        public int YearBuilt { get; init; }

        [JsonPropertyName("yr_renovated")]
        public int YearRenovated { get; init; }

        [JsonPropertyName("zipcode")]
        public int Zipcode { get; init; }

        [JsonPropertyName("lat")]
        public decimal Latitude { get; init; }

        [JsonPropertyName("long")]
        public decimal Longitude { get; init; }

        [JsonPropertyName("sqft_living15")]
        public int SqftLiving15 { get; init; }

        [JsonPropertyName("sqft_lot15")]
        public int SqftLot15 { get; init; }

        [JsonPropertyName("sale_year")]
        public int SaleYear { get; init; }

        [JsonPropertyName("sale_month")]
        public int SaleMonth { get; init; }
    }

    private sealed class MlPricePredictionResponse
    {
        [JsonPropertyName("predicted_price")]
        public decimal PredictedPrice { get; init; }

        [JsonPropertyName("predicted_price_formatted")]
        public string? PredictedPriceFormatted { get; init; }
    }
}
