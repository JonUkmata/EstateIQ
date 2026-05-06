namespace EstateIQ.Models;

public class FileRecord
{
    public Guid Id { get; set; }

    public string Entity { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public Guid? UploadedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? UploadedByUser { get; set; }
}
