namespace EstateIQ.DTOs.Files;

public class FileResponseDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }
}
