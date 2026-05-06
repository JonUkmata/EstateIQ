using Microsoft.AspNetCore.Http;

namespace EstateIQ.Interfaces;

public interface IFileValidationService
{
    void ValidatePropertyImages(IReadOnlyCollection<IFormFile> files, int existingImageCount);
}
