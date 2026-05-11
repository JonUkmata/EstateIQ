using EstateIQ.DTOs.Files;
using Microsoft.AspNetCore.Http;

namespace EstateIQ.Interfaces;

public interface IPropertyImageService
{
    Task<IReadOnlyList<UploadedFileDto>> UploadImagesAsync(int propertyId, IReadOnlyCollection<IFormFile> files, Guid? uploadedBy);

    Task<IReadOnlyList<FileResponseDto>> GetImagesAsync(int propertyId);

    Task DeleteImageAsync(int propertyId, Guid imageId);
}
