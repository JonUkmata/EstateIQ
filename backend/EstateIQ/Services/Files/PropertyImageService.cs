using EstateIQ.DTOs.Files;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Services.Files;

public class PropertyImageService(
    IPropertyRepository propertyRepository,
    IFileRepository fileRepository,
    IWebHostEnvironment webHostEnvironment,
    ILogger<PropertyImageService> logger) : IPropertyImageService
{
    private const string PropertyEntity = "Property";
    private const int MaxImagesPerProperty = 10;
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IPropertyRepository _propertyRepository = propertyRepository;
    private readonly IFileRepository _fileRepository = fileRepository;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly ILogger<PropertyImageService> _logger = logger;

    public async Task<IReadOnlyList<UploadedFileDto>> UploadImagesAsync(int propertyId, IReadOnlyCollection<IFormFile> files, Guid? uploadedBy)
    {
        if (!await _propertyRepository.ExistsAsync(propertyId))
        {
            throw new NotFoundException($"Property with id {propertyId} was not found.");
        }

        ValidateFiles(files);

        var entityId = CreatePropertyEntityId(propertyId);
        var existingImageCount = await _fileRepository.CountByEntityAsync(PropertyEntity, entityId);

        if (existingImageCount + files.Count > MaxImagesPerProperty)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(files)] = [$"A property can have at most {MaxImagesPerProperty} images."]
            });
        }

        var uploadsRoot = GetUploadsRoot(propertyId);
        Directory.CreateDirectory(uploadsRoot);

        var createdFiles = new List<string>();
        var records = new List<FileRecord>();

        try
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var generatedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var destinationPath = Path.Combine(uploadsRoot, generatedFileName);

                await using (var stream = new FileStream(destinationPath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                createdFiles.Add(destinationPath);

                records.Add(new FileRecord
                {
                    Id = Guid.NewGuid(),
                    Entity = PropertyEntity,
                    EntityId = entityId,
                    FileName = file.FileName,
                    FilePath = $"/uploads/properties/{propertyId}/{generatedFileName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadedBy = uploadedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _fileRepository.AddRangeAsync(records);
        }
        catch (Exception exception)
        {
            DeleteCreatedFiles(createdFiles);
            _logger.LogError(exception, "Error uploading images for property {PropertyId}.", propertyId);
            throw;
        }

        return records
            .Select(file => new UploadedFileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            })
            .ToList();
    }

    private static void ValidateFiles(IReadOnlyCollection<IFormFile> files)
    {
        var errors = new Dictionary<string, string[]>();

        if (files.Count == 0)
        {
            errors[nameof(files)] = ["At least one file is required."];
            throw new ValidationException(errors);
        }

        var fileErrors = new List<string>();

        foreach (var file in files)
        {
            if (file.Length <= 0)
            {
                fileErrors.Add($"{file.FileName}: File cannot be empty.");
            }

            if (file.Length > MaxFileSize)
            {
                fileErrors.Add($"{file.FileName}: File size cannot exceed 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (!AllowedExtensions.Contains(extension))
            {
                fileErrors.Add($"{file.FileName}: File extension is not allowed.");
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                fileErrors.Add($"{file.FileName}: Content type is not allowed.");
            }
        }

        if (fileErrors.Count > 0)
        {
            errors[nameof(files)] = fileErrors.Distinct().ToArray();
            throw new ValidationException(errors);
        }
    }

    private string GetUploadsRoot(int propertyId)
    {
        var webRootPath = string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath)
            ? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot")
            : _webHostEnvironment.WebRootPath;

        return Path.Combine(webRootPath, "uploads", "properties", propertyId.ToString());
    }

    private static Guid CreatePropertyEntityId(int propertyId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(propertyId).CopyTo(bytes, 0);

        return new Guid(bytes);
    }

    private static void DeleteCreatedFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort cleanup after a failed upload.
            }
        }
    }
}
