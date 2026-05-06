using EstateIQ.Exceptions;
using EstateIQ.Services.Files;
using Microsoft.AspNetCore.Http;
using Xunit;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Tests;

public class FileValidationServiceTests
{
    private readonly FileValidationService _service = new();

    [Fact]
    public void ValidatePropertyImages_WithInvalidExtension_ThrowsValidationException()
    {
        var file = BuildFormFile("image.txt", "image/jpeg", 1024);

        var exception = Assert.Throws<ValidationException>(() =>
            _service.ValidatePropertyImages([file], existingImageCount: 0));

        Assert.Contains(exception.Errors["files"], error => error.Contains("File extension is not allowed."));
    }

    [Fact]
    public void ValidatePropertyImages_WithInvalidContentType_ThrowsValidationException()
    {
        var file = BuildFormFile("image.jpg", "text/plain", 1024);

        var exception = Assert.Throws<ValidationException>(() =>
            _service.ValidatePropertyImages([file], existingImageCount: 0));

        Assert.Contains(exception.Errors["files"], error => error.Contains("Content type is not allowed."));
    }

    [Fact]
    public void ValidatePropertyImages_WithOversizedFile_ThrowsValidationException()
    {
        var file = BuildFormFile("image.jpg", "image/jpeg", (5 * 1024 * 1024) + 1);

        var exception = Assert.Throws<ValidationException>(() =>
            _service.ValidatePropertyImages([file], existingImageCount: 0));

        Assert.Contains(exception.Errors["files"], error => error.Contains("File size cannot exceed 5 MB."));
    }

    [Fact]
    public void ValidatePropertyImages_WithEmptyFile_ThrowsValidationException()
    {
        var file = BuildFormFile("image.jpg", "image/jpeg", 0);

        var exception = Assert.Throws<ValidationException>(() =>
            _service.ValidatePropertyImages([file], existingImageCount: 0));

        Assert.Contains(exception.Errors["files"], error => error.Contains("File cannot be empty."));
    }

    [Fact]
    public void ValidatePropertyImages_WhenMoreThanTenTotalImages_ThrowsValidationException()
    {
        var file = BuildFormFile("image.jpg", "image/jpeg", 1024);

        var exception = Assert.Throws<ValidationException>(() =>
            _service.ValidatePropertyImages([file], existingImageCount: 10));

        Assert.Contains(exception.Errors["files"], error => error.Contains("at most 10 images"));
    }

    [Fact]
    public void ValidatePropertyImages_WithValidImage_DoesNotThrow()
    {
        var file = BuildFormFile("image.webp", "image/webp", 1024);

        _service.ValidatePropertyImages([file], existingImageCount: 9);
    }

    private static IFormFile BuildFormFile(string fileName, string contentType, long length)
    {
        var file = new FormFile(Stream.Null, 0, length, "files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        return file;
    }
}
