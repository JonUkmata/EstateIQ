using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

public class FileRepository(AppDbContext dbContext) : IFileRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<int> CountByEntityAsync(string entity, Guid entityId)
    {
        return await _dbContext.Files
            .AsNoTracking()
            .CountAsync(file => file.Entity == entity && file.EntityId == entityId);
    }

    public async Task<IReadOnlyList<FileRecord>> GetByEntityAsync(string entity, Guid entityId)
    {
        return await _dbContext.Files
            .AsNoTracking()
            .Where(file => file.Entity == entity && file.EntityId == entityId)
            .OrderBy(file => file.CreatedAt)
            .ThenBy(file => file.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<Guid, FileRecord>> GetFirstByEntitiesAsync(string entity, IReadOnlyCollection<Guid> entityIds)
    {
        if (entityIds.Count == 0)
        {
            return new Dictionary<Guid, FileRecord>();
        }

        var files = await _dbContext.Files
            .AsNoTracking()
            .Where(file => file.Entity == entity && entityIds.Contains(file.EntityId))
            .OrderBy(file => file.CreatedAt)
            .ThenBy(file => file.Id)
            .ToListAsync();

        return files
            .GroupBy(file => file.EntityId)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public async Task<FileRecord?> GetByEntityAndIdAsync(string entity, Guid entityId, Guid id)
    {
        return await _dbContext.Files
            .AsNoTracking()
            .SingleOrDefaultAsync(file => file.Entity == entity && file.EntityId == entityId && file.Id == id);
    }

    public async Task AddRangeAsync(IEnumerable<FileRecord> files)
    {
        await _dbContext.Files.AddRangeAsync(files);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(FileRecord file)
    {
        _dbContext.Files.Remove(file);
        await _dbContext.SaveChangesAsync();
    }
}
