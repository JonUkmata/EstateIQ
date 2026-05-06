using EstateIQ.Constants;
using EstateIQ.Interfaces;
using Microsoft.AspNetCore.Http;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Services.Files;

public class FileValidationService : IFileValidationService
{
    private static readonly HashSet<string> AllowedExtensions = new(
        FileUploadConstants.AllowedImageExtensions,
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedContentTypes = new(
        FileUploadConstants.AllowedImageContentTypes,
        StringComparer.OrdinalIgnoreCase);

    public void ValidatePropertyImages(IReadOnlyCollection<IFormFile> files, int existingImageCount)
    {
        var errors = new Dictionary<string, string[]>();

        if (files.Count == 0)
        {
            errors[nameof(files)] = ["At least one file is required."];
            throw new ValidationException(errors);
        }

        if (existingImageCount + files.Count > FileUploadConstants.MaxPropertyImages)
        {
            errors[nameof(files)] =
            [
                $"A property can have at most {FileUploadConstants.MaxPropertyImages} images."
            ];
        }

        var fileErrors = new List<string>();

        foreach (var file in files)
        {
            ValidateFile(file, fileErrors);
        }

        if (fileErrors.Count > 0)
        {
            errors[nameof(files)] = [.. errors.GetValueOrDefault(nameof(files), []), .. fileErrors.Distinct()];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void ValidateFile(IFormFile file, ICollection<string> errors)
    {
        var fileName = string.IsNullOrWhiteSpace(file.FileName)
            ? "File"
            : file.FileName;

        if (file.Length <= 0)
        {
            errors.Add($"{fileName}: File cannot be empty.");
        }

        if (file.Length > FileUploadConstants.MaxImageFileSizeBytes)
        {
            errors.Add($"{fileName}: File size cannot exceed 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            errors.Add($"{fileName}: File extension is not allowed. Allowed extensions: jpg, jpeg, png, webp.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            errors.Add($"{fileName}: Content type is not allowed. Allowed content types: image/jpeg, image/png, image/webp.");
        }
    }
}
