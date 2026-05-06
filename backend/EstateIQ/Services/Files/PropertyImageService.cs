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
    IFileValidationService fileValidationService,
    IWebHostEnvironment webHostEnvironment,
    ILogger<PropertyImageService> logger) : IPropertyImageService
{
    private const string PropertyEntity = "Property";

    private readonly IPropertyRepository _propertyRepository = propertyRepository;
    private readonly IFileRepository _fileRepository = fileRepository;
    private readonly IFileValidationService _fileValidationService = fileValidationService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly ILogger<PropertyImageService> _logger = logger;

    public async Task<IReadOnlyList<UploadedFileDto>> UploadImagesAsync(int propertyId, IReadOnlyCollection<IFormFile> files, Guid? uploadedBy)
    {
        if (!await _propertyRepository.ExistsAsync(propertyId))
        {
            throw new NotFoundException($"Property with id {propertyId} was not found.");
        }

        var entityId = CreatePropertyEntityId(propertyId);
        var existingImageCount = await _fileRepository.CountByEntityAsync(PropertyEntity, entityId);
        _fileValidationService.ValidatePropertyImages(files, existingImageCount);

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

    public async Task<IReadOnlyList<FileResponseDto>> GetImagesAsync(int propertyId)
    {
        if (!await _propertyRepository.ExistsAsync(propertyId))
        {
            throw new NotFoundException($"Property with id {propertyId} was not found.");
        }

        var entityId = CreatePropertyEntityId(propertyId);
        var files = await _fileRepository.GetByEntityAsync(PropertyEntity, entityId);

        return files
            .Select(file => new FileResponseDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Url = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            })
            .ToList();
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
