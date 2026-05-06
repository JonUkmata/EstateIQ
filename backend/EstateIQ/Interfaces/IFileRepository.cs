using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IFileRepository
{
    Task<int> CountByEntityAsync(string entity, Guid entityId);

    Task AddRangeAsync(IEnumerable<FileRecord> files);
}
