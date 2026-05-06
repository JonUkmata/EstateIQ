using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IFileRepository
{
    Task<int> CountByEntityAsync(string entity, Guid entityId);

    Task<IReadOnlyList<FileRecord>> GetByEntityAsync(string entity, Guid entityId);

    Task<FileRecord?> GetByEntityAndIdAsync(string entity, Guid entityId, Guid id);

    Task AddRangeAsync(IEnumerable<FileRecord> files);

    Task DeleteAsync(FileRecord file);
}
