using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IUserRepository
{
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
}
