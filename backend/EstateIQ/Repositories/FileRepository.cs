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

    public async Task AddRangeAsync(IEnumerable<FileRecord> files)
    {
        await _dbContext.Files.AddRangeAsync(files);
        await _dbContext.SaveChangesAsync();
    }
}
