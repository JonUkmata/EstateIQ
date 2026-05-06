namespace EstateIQ.Constants;

public static class FileUploadConstants
{
    public const int MaxPropertyImages = 10;

    public const long MaxImageFileSizeBytes = 5 * 1024 * 1024;

    public static readonly string[] AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public static readonly string[] AllowedImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
