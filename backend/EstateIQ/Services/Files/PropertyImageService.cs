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

    public async Task<IReadOnlyDictionary<int, string>> GetCoverImageUrlsAsync(IReadOnlyCollection<int> propertyIds)
    {
        if (propertyIds.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        var entityIdToPropertyId = propertyIds
            .Distinct()
            .ToDictionary(CreatePropertyEntityId, propertyId => propertyId);

        var firstFiles = await _fileRepository.GetFirstByEntitiesAsync(PropertyEntity, entityIdToPropertyId.Keys.ToList());

        var coverImageUrls = new Dictionary<int, string>();

        foreach (var (entityId, file) in firstFiles)
        {
            if (entityIdToPropertyId.TryGetValue(entityId, out var propertyId))
            {
                coverImageUrls[propertyId] = file.FilePath;
            }
        }

        return coverImageUrls;
    }

    public async Task DeleteImageAsync(int propertyId, Guid imageId)
    {
        if (!await _propertyRepository.ExistsAsync(propertyId))
        {
            throw new NotFoundException($"Property with id {propertyId} was not found.");
        }

        var entityId = CreatePropertyEntityId(propertyId);
        var file = await _fileRepository.GetByEntityAndIdAsync(PropertyEntity, entityId, imageId);

        if (file is null)
        {
            throw new NotFoundException($"Image with id {imageId} was not found for property {propertyId}.");
        }

        DeletePhysicalFile(file.FilePath);
        await _fileRepository.DeleteAsync(file);
    }

    private string GetUploadsRoot(int propertyId)
    {
        var webRootPath = string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath)
            ? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot")
            : _webHostEnvironment.WebRootPath;

        return Path.Combine(webRootPath, "uploads", "properties", propertyId.ToString());
    }

    private string GetWebRootPath()
    {
        return string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath)
            ? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot")
            : _webHostEnvironment.WebRootPath;
    }

    private void DeletePhysicalFile(string filePath)
    {
        try
        {
            var webRootPath = Path.GetFullPath(GetWebRootPath());
            var relativeFilePath = filePath.TrimStart('/', '\\');
            var absoluteFilePath = Path.GetFullPath(Path.Combine(webRootPath, relativeFilePath));

            if (!absoluteFilePath.StartsWith(webRootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Skipping deletion for file path outside web root: {FilePath}.", filePath);
                return;
            }

            if (File.Exists(absoluteFilePath))
            {
                File.Delete(absoluteFilePath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not delete physical file {FilePath}. Metadata will still be removed.", filePath);
        }
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
