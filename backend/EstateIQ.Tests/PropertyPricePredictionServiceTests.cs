using System.Net;
using System.Text;
using System.Text.Json;
using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EstateIQ.Tests;

public class PropertyPricePredictionServiceTests
{
    [Fact]
    public async Task GenerateAsync_MapsAgentFormToMlPayload()
    {
        var handler = new CapturingHandler("""{"predicted_price":549133.56,"predicted_price_formatted":"$549,134"}""");
        var service = CreateService(handler);

        var result = await service.GenerateAsync(new GeneratePropertyPriceRequestDto
        {
            LivingArea = 100,
            LivingAreaUnit = "m2",
            Bedrooms = 3,
            Bathrooms = 2.5m,
            Floors = 1.5m,
            YearBuilt = 2020,
            Latitude = 47.61m,
            Longitude = -122.33m,
            Zipcode = 98101,
            BasementArea = 10,
            BasementAreaUnit = "m2",
            HasBasement = true,
            NearbyLivingArea = 120,
            NearbyLivingAreaUnit = "m2",
            NearbyLotArea = 2000,
            NearbyLotAreaUnit = "sqft"
        });

        Assert.Equal(549133.56m, result.SuggestedPrice);
        Assert.Equal("$549,134", result.FormattedPrice);
        Assert.Contains("Year built is outside the model training range.", result.MlWarnings);

        using var document = JsonDocument.Parse(handler.RequestBody);
        var root = document.RootElement;
        Assert.Equal(1076, root.GetProperty("sqft_living").GetInt32());
        Assert.Equal(108, root.GetProperty("sqft_basement").GetInt32());
        Assert.Equal(968, root.GetProperty("sqft_above").GetInt32());
        Assert.Equal(1076, root.GetProperty("sqft_lot").GetInt32());
        Assert.Equal(1292, root.GetProperty("sqft_living15").GetInt32());
        Assert.Equal(2000, root.GetProperty("sqft_lot15").GetInt32());
        Assert.Equal(0, root.GetProperty("waterfront").GetInt32());
        Assert.Equal(0, root.GetProperty("view").GetInt32());
        Assert.Equal(3, root.GetProperty("condition").GetInt32());
        Assert.Equal(7, root.GetProperty("grade").GetInt32());
        Assert.Equal(2014, root.GetProperty("sale_year").GetInt32());
        Assert.Equal(11, root.GetProperty("sale_month").GetInt32());
    }

    [Fact]
    public async Task GenerateAsync_BasementLargerThanLivingArea_ThrowsValidationException()
    {
        var service = CreateService(new CapturingHandler("""{"predicted_price":1,"predicted_price_formatted":"$1"}"""));

        var request = new GeneratePropertyPriceRequestDto
        {
            LivingArea = 50,
            LivingAreaUnit = "m2",
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2000,
            Latitude = 47.61m,
            Longitude = -122.33m,
            Zipcode = 98101,
            HasBasement = true,
            BasementArea = 60,
            BasementAreaUnit = "m2"
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.GenerateAsync(request));

        Assert.Contains(nameof(GeneratePropertyPriceRequestDto.BasementArea), exception.Errors.Keys);
    }

    private static PropertyPricePredictionService CreateService(CapturingHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ml:PredictUrl"] = "http://ml.test/predict"
            })
            .Build();

        return new PropertyPricePredictionService(
            new HttpClient(handler),
            configuration,
            NullLogger<PropertyPricePredictionService>.Instance);
    }

    private sealed class CapturingHandler(string responseBody) : HttpMessageHandler
    {
        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
