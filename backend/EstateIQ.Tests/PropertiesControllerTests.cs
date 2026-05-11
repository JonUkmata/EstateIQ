using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EstateIQ.Constants;
using EstateIQ.Data;
using EstateIQ.DTOs;
using EstateIQ.DTOs.Auth;
using EstateIQ.DTOs.Files;
using EstateIQ.Models;
using EstateIQ.Services.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EstateIQ.Tests;

public class PropertiesControllerTests
{
    private static readonly JwtSettings TestJwtSettings = new()
    {
        Issuer = "EstateIQ",
        Audience = "EstateIQ",
        Key = "EstateIQ-Development-Jwt-Key-Replace-In-Production-2026",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    [Fact]
    public async Task GetProperties_ReturnsSeededProperties()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.TryGetProperty("items", out _));
        Assert.True(document.RootElement.TryGetProperty("totalCount", out _));
        Assert.True(document.RootElement.TryGetProperty("page", out _));
        Assert.True(document.RootElement.TryGetProperty("pageSize", out _));
        Assert.True(document.RootElement.TryGetProperty("totalPages", out _));
        Assert.False(document.RootElement.TryGetProperty("pageNumber", out _));

        var result = JsonSerializer.Deserialize<PagedResult<PropertyDto>>(content, JsonSerializerOptions.Web);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);

        var property = result.Items.Single();
        Assert.Equal("Modern Apartment", property.Title);
        Assert.Equal("Apartment", property.PropertyType.Name);
        Assert.Equal(41.3275m, property.Latitude);
        Assert.Equal(19.8187m, property.Longitude);
    }

    [Fact]
    public async Task GetProperties_WithQueryParameters_ReturnsFilteredPage()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertiesAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties?city=Tirane&propertyTypeId=1&propertyStatusId=1&minPrice=100000&maxPrice=150000&search=renovated&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PropertyDto>>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Modern Apartment", result.Items.Single().Title);
    }

    [Fact]
    public async Task GetProperty_ExistingId_ReturnsCompletePropertyDetails()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal(propertyId, result!.Id);
        Assert.Equal("Modern Apartment", result.Title);
        Assert.Equal("Freshly renovated apartment", result.Description);
        Assert.Equal(120000m, result.Price);
        Assert.Equal(2, result.Bedrooms);
        Assert.Equal(1, result.Bathrooms);
        Assert.Equal(78m, result.Area);
        Assert.Equal("Rruga e Kavajes", result.Address);
        Assert.Equal("Tirane", result.City);
        Assert.Equal("Apartment", result.PropertyType.Name);
        Assert.Equal("For Sale", result.PropertyStatus.Name);
        Assert.Equal("EstateIQ", result.Company.Name);
        Assert.Equal("Valon", result.Agent.FirstName);
        Assert.Equal("Dobrunaj", result.Agent.LastName);
        Assert.Equal(41.3275m, result.Latitude);
        Assert.Equal(19.8187m, result.Longitude);
        Assert.Empty(result.Images);
    }

    [Fact]
    public async Task GetProperty_MissingId_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_ValidPayload_ReturnsCreatedProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.CreateProperty);

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal("Modern Apartment", result!.Title);
        Assert.Equal("For Sale", result.PropertyStatus.Name);
        Assert.Equal(41.3275m, result.Latitude);
        Assert.Equal(19.8187m, result.Longitude);
    }

    [Fact]
    public async Task UpdateProperty_ValidPayload_ReturnsUpdatedProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.EditProperty);

        var updateDto = BuildUpdateDto();
        var response = await client.PutAsJsonAsync($"/api/properties/{propertyId}", updateDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal(propertyId, result!.Id);
        Assert.Equal("Updated Apartment", result.Title);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(145000m, result.Price);
        Assert.Equal(82m, result.Area);
        Assert.Equal(3, result.Bedrooms);
        Assert.Equal(2, result.Bathrooms);
        Assert.Equal("Rruga e Portit", result.Address);
        Assert.Equal("Durres", result.City);
        Assert.Equal("Apartment", result.PropertyType.Name);
        Assert.Equal("For Sale", result.PropertyStatus.Name);
        Assert.Equal("EstateIQ", result.Company.Name);
        Assert.Equal("Valon", result.Agent.FirstName);
        Assert.Equal("Dobrunaj", result.Agent.LastName);
        Assert.Equal(41.323m, result.Latitude);
        Assert.Equal(19.441m, result.Longitude);
    }

    [Fact]
    public async Task UpdateProperty_InvalidPayload_ReturnsBadRequest()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.EditProperty);

        var invalidDto = BuildUpdateDto();
        invalidDto.Title = string.Empty;
        invalidDto.Price = 0;

        var response = await client.PutAsJsonAsync($"/api/properties/{propertyId}", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProperty_MissingProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.EditProperty);

        var updateDto = BuildUpdateDto();
        var response = await client.PutAsJsonAsync("/api/properties/999", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProperty_ExistingProperty_ReturnsNoContentAndDeletesProperty()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.DeleteProperty);

        var response = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/properties/{propertyId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProperty_MissingProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.DeleteProperty);

        var response = await client.DeleteAsync("/api/properties/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProperty_UnderContractProperty_ReturnsConflict()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync(propertyStatusId: 6);
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.DeleteProperty);

        var response = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Sold, rented, or under-contract properties cannot be deleted.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateProperty_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_WithUserPermissionSet_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties, Permissions.BookViewing);

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_WithRealLoggedInUserRole_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedLoginReadyUserAsync(Roles.User, "public.user@example.com");
        using var client = factory.CreateClient();
        await LoginAndAttachTokenAsync(client, "public.user@example.com");

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Roles.Admin, "admin.property@example.com")]
    [InlineData(Roles.CompanyAdmin, "company.admin.property@example.com")]
    [InlineData(Roles.Agent, "agent.property@example.com")]
    public async Task CreateProperty_WithRealLoggedInManagementRole_ReturnsCreated(string roleName, string email)
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedLoginReadyUserAsync(roleName, email);
        using var client = factory.CreateClient();
        await LoginAndAttachTokenAsync(client, email);

        var response = await client.PostAsJsonAsync("/api/properties", BuildCreateDto());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal("Modern Apartment", result!.Title);
    }

    [Fact]
    public async Task WriteEndpoints_WithoutToken_ReturnUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var putResponse = await client.PutAsJsonAsync($"/api/properties/{propertyId}", BuildUpdateDto());
        var deleteResponse = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.Unauthorized, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task WriteEndpoints_WithUserPermissionSet_ReturnForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties, Permissions.BookViewing);

        var putResponse = await client.PutAsJsonAsync($"/api/properties/{propertyId}", BuildUpdateDto());
        var deleteResponse = await client.DeleteAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task UploadImages_WithValidImage_SavesFileAndMetadata()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var content = BuildImageUploadContent(("image.jpg", "image/jpeg"));

        var response = await client.PostAsync($"/api/properties/{propertyId}/images", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<UploadedFileDto>>();
        Assert.NotNull(result);
        var uploadedFile = Assert.Single(result!);
        Assert.Equal("image.jpg", uploadedFile.FileName);
        Assert.Equal("image/jpeg", uploadedFile.ContentType);
        Assert.Equal(4, uploadedFile.FileSize);
        Assert.StartsWith($"/uploads/properties/{propertyId}/", uploadedFile.FilePath);

        var relativeFilePath = uploadedFile.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        Assert.True(File.Exists(Path.Combine(factory.WebRootPath, relativeFilePath)));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedFile = Assert.Single(dbContext.Files);
        Assert.Equal(uploadedFile.Id, storedFile.Id);
        Assert.Equal("Property", storedFile.Entity);
        Assert.Equal(uploadedFile.FilePath, storedFile.FilePath);
    }

    [Fact]
    public async Task UploadImages_WhenMoreThanTenImages_ReturnsBadRequest()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var content = BuildImageUploadContent(
            ("image-1.jpg", "image/jpeg"),
            ("image-2.jpg", "image/jpeg"),
            ("image-3.jpg", "image/jpeg"),
            ("image-4.jpg", "image/jpeg"),
            ("image-5.jpg", "image/jpeg"),
            ("image-6.jpg", "image/jpeg"),
            ("image-7.jpg", "image/jpeg"),
            ("image-8.jpg", "image/jpeg"),
            ("image-9.jpg", "image/jpeg"),
            ("image-10.jpg", "image/jpeg"),
            ("image-11.jpg", "image/jpeg"));

        var response = await client.PostAsync($"/api/properties/{propertyId}/images", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
        Assert.False(Directory.Exists(Path.Combine(factory.WebRootPath, "uploads", "properties", propertyId.ToString())));
    }

    [Fact]
    public async Task UploadImages_WithInvalidExtension_ReturnsBadRequest()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var content = BuildImageUploadContent(("image.gif", "image/gif"));

        var response = await client.PostAsync($"/api/properties/{propertyId}/images", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
        Assert.False(Directory.Exists(Path.Combine(factory.WebRootPath, "uploads", "properties", propertyId.ToString())));
    }

    [Fact]
    public async Task UploadImages_MissingProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var content = BuildImageUploadContent(("image.jpg", "image/jpeg"));

        var response = await client.PostAsync("/api/properties/999/images", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
        Assert.False(Directory.Exists(Path.Combine(factory.WebRootPath, "uploads", "properties", "999")));
    }

    [Fact]
    public async Task UploadImages_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        using var content = BuildImageUploadContent(("image.jpg", "image/jpeg"));

        var response = await client.PostAsync($"/api/properties/{propertyId}/images", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
        Assert.False(Directory.Exists(Path.Combine(factory.WebRootPath, "uploads", "properties", propertyId.ToString())));
    }

    [Fact]
    public async Task UploadImages_WithUserPermissionSet_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties, Permissions.BookViewing);
        using var content = BuildImageUploadContent(("image.jpg", "image/jpeg"));

        var response = await client.PostAsync($"/api/properties/{propertyId}/images", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
        Assert.False(Directory.Exists(Path.Combine(factory.WebRootPath, "uploads", "properties", propertyId.ToString())));
    }

    [Fact]
    public async Task GetImages_WithUploadedImages_ReturnsPublicImageMetadata()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var uploadContent = BuildImageUploadContent(("image.jpg", "image/jpeg"));
        var uploadResponse = await client.PostAsync($"/api/properties/{propertyId}/images", uploadContent);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync($"/api/properties/{propertyId}/images");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<FileResponseDto>>();
        Assert.NotNull(result);
        var image = Assert.Single(result!);
        Assert.Equal("image.jpg", image.FileName);
        Assert.Equal("image/jpeg", image.ContentType);
        Assert.Equal(4, image.FileSize);
        Assert.StartsWith($"/uploads/properties/{propertyId}/", image.Url);
    }

    [Fact]
    public async Task GetImages_WithNoImages_ReturnsEmptyList()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/properties/{propertyId}/images");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<FileResponseDto>>();
        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetImages_MissingProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedReferenceDataAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties/999/images");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteImage_WithValidImage_ReturnsNoContentAndRemovesFileAndMetadata()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var uploadContent = BuildImageUploadContent(("image.jpg", "image/jpeg"));
        var uploadResponse = await client.PostAsync($"/api/properties/{propertyId}/images", uploadContent);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploadedFile = Assert.Single(await uploadResponse.Content.ReadFromJsonAsync<List<UploadedFileDto>>() ?? []);
        var relativeFilePath = uploadedFile.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absoluteFilePath = Path.Combine(factory.WebRootPath, relativeFilePath);
        Assert.True(File.Exists(absoluteFilePath));
        AddBearerToken(client, Permissions.EditProperty);

        var response = await client.DeleteAsync($"/api/properties/{propertyId}/images/{uploadedFile.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(File.Exists(absoluteFilePath));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(dbContext.Files);
    }

    [Fact]
    public async Task DeleteImage_FromWrongProperty_ReturnsNotFound()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        await factory.SeedPropertiesAsync();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var propertyIds = await dbContext.Properties
            .OrderBy(property => property.Id)
            .Select(property => property.Id)
            .ToListAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var uploadContent = BuildImageUploadContent(("image.jpg", "image/jpeg"));
        var uploadResponse = await client.PostAsync($"/api/properties/{propertyIds[0]}/images", uploadContent);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploadedFile = Assert.Single(await uploadResponse.Content.ReadFromJsonAsync<List<UploadedFileDto>>() ?? []);

        var response = await client.DeleteAsync($"/api/properties/{propertyIds[1]}/images/{uploadedFile.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(dbContext.Files);
    }

    [Fact]
    public async Task DeleteImage_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/properties/{propertyId}/images/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteImage_WithUserPermissionSet_ReturnsForbidden()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.ViewProperties, Permissions.BookViewing);

        var response = await client.DeleteAsync($"/api/properties/{propertyId}/images/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProperty_WithUploadedImages_IncludesImagesInDetails()
    {
        await using var factory = new EstateIqWebApplicationFactory();
        var propertyId = await factory.SeedPropertyAsync();
        using var client = factory.CreateClient();
        AddBearerToken(client, Permissions.UploadPropertyImages);
        using var uploadContent = BuildImageUploadContent(("image.webp", "image/webp"));
        var uploadResponse = await client.PostAsync($"/api/properties/{propertyId}/images", uploadContent);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync($"/api/properties/{propertyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.NotNull(result);
        Assert.Equal(propertyId, result!.Id);
        var image = Assert.Single(result.Images);
        Assert.Equal("image.webp", image.FileName);
        Assert.StartsWith($"/uploads/properties/{propertyId}/", image.Url);
    }

    private sealed class EstateIqWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();
        private readonly string _webRootPath = Path.Combine(Path.GetTempPath(), $"estateiq-tests-{Guid.NewGuid():N}");

        public string WebRootPath => _webRootPath;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseWebRoot(_webRootPath);

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EstateIQTests;Trusted_Connection=True;TrustServerCertificate=True",
                    ["Redis:ConnectionString"] = "localhost:6379"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && Directory.Exists(_webRootPath))
            {
                Directory.Delete(_webRootPath, recursive: true);
            }
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task SeedReferenceDataAsync()
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);
        }

        public async Task<int> SeedPropertyAsync(int propertyStatusId = 1)
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);

            var property = new Property
            {
                Title = "Modern Apartment",
                Description = "Freshly renovated apartment",
                Price = 120000m,
                Area = 78m,
                Bedrooms = 2,
                Bathrooms = 1,
                Floors = 1,
                YearBuilt = 2021,
                PropertyTypeId = 1,
                PropertyStatusId = propertyStatusId,
                CompanyId = 1,
                AgentId = 1,
                Address = "Rruga e Kavajes",
                City = "Tirane",
                Latitude = 41.3275m,
                Longitude = 19.8187m,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            return property.Id;
        }

        public async Task SeedPropertiesAsync()
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedReferenceDataAsync(dbContext);

            dbContext.Properties.AddRange(
                new Property
                {
                    Title = "Modern Apartment",
                    Description = "Freshly renovated apartment",
                    Price = 120000m,
                    Area = 78m,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Floors = 1,
                    YearBuilt = 2021,
                    PropertyTypeId = 1,
                    PropertyStatusId = 1,
                    CompanyId = 1,
                    AgentId = 1,
                    Address = "Rruga e Kavajes",
                    City = "Tirane",
                    Latitude = 41.3275m,
                    Longitude = 19.8187m,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Title = "Coastal Apartment",
                    Description = "Sea view apartment",
                    Price = 180000m,
                    Area = 92m,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Floors = 1,
                    YearBuilt = 2019,
                    PropertyTypeId = 1,
                    PropertyStatusId = 1,
                    CompanyId = 1,
                    AgentId = 1,
                    Address = "Rruga Taulantia",
                    City = "Durres",
                    Latitude = 41.3133m,
                    Longitude = 19.4469m,
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();
        }

        public async Task SeedLoginReadyUserAsync(string roleName, string email)
        {
            await ResetDatabaseAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedReferenceDataAsync(dbContext);

            var role = await dbContext.Roles.SingleAsync(existingRole => existingRole.Name == roleName);
            var passwordService = new PasswordService();
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Auth",
                LastName = roleName,
                Email = email,
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = passwordService.HashPassword(user, "Password123!");

            dbContext.Users.Add(user);
            dbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedReferenceDataAsync(AppDbContext dbContext)
        {
            if (!await dbContext.PropertyTypes.AnyAsync(propertyType => propertyType.Id == 1))
            {
                dbContext.PropertyTypes.Add(new PropertyType
                {
                    Id = 1,
                    Name = "Apartment",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (!await dbContext.PropertyStatuses.AnyAsync(propertyStatus => propertyStatus.Id == 1))
            {
                dbContext.PropertyStatuses.Add(new PropertyStatus
                {
                    Id = 1,
                    Name = "For Sale",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (!await dbContext.PropertyStatuses.AnyAsync(propertyStatus => propertyStatus.Id == 6))
            {
                dbContext.PropertyStatuses.Add(new PropertyStatus
                {
                    Id = 6,
                    Name = "Under Contract",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            dbContext.Companies.Add(new Company
            {
                Id = 1,
                Name = "EstateIQ",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            dbContext.Agents.Add(new Agent
            {
                Id = 1,
                FirstName = "Valon",
                LastName = "Dobrunaj",
                Email = "valon@estateiq.local",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            dbContext.AgentCompanies.Add(new AgentCompany
            {
                Id = 1,
                AgentId = 1,
                CompanyId = 1,
                Role = "Senior Agent",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static CreatePropertyDto BuildCreateDto()
    {
        return new CreatePropertyDto
        {
            Title = "Modern Apartment",
            Description = "Freshly renovated apartment",
            Price = 120000m,
            Area = 78m,
            Bedrooms = 2,
            Bathrooms = 1,
            Floors = 1,
            YearBuilt = 2021,
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            CompanyId = 1,
            AgentId = 1,
            Address = "Rruga e Kavajes",
            City = "Tirane",
            Latitude = 41.3275m,
            Longitude = 19.8187m
        };
    }

    private static UpdatePropertyDto BuildUpdateDto()
    {
        return new UpdatePropertyDto
        {
            Id = 0,
            Title = "Updated Apartment",
            Description = "Updated description",
            Price = 145000m,
            Area = 82m,
            Bedrooms = 3,
            Bathrooms = 2,
            Floors = 1,
            YearBuilt = 2022,
            PropertyTypeId = 1,
            PropertyStatusId = 1,
            CompanyId = 1,
            AgentId = 1,
            Address = "Rruga e Portit",
            City = "Durres",
            Latitude = 41.323m,
            Longitude = 19.441m
        };
    }

    private static void AddBearerToken(HttpClient client, params string[] permissions)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(permissions));
    }

    private static string GenerateToken(params string[] permissions)
    {
        var tokenService = new TokenService(Options.Create(TestJwtSettings));

        return tokenService.GenerateAccessToken(
            new User
            {
                Id = Guid.NewGuid(),
                Email = "property-test@example.com"
            },
            [Roles.Agent],
            permissions);
    }

    private static async Task LoginAndAttachTokenAsync(HttpClient client, string email)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = email,
            Password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private static MultipartFormDataContent BuildImageUploadContent(params (string FileName, string ContentType)[] files)
    {
        var content = new MultipartFormDataContent();

        foreach (var file in files)
        {
            var fileContent = new ByteArrayContent([1, 2, 3, 4]);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "files", file.FileName);
        }

        return content;
    }
}
