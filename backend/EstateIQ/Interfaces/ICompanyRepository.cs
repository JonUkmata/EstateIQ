namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for companies.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Gets all active companies sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Company>> GetAllActiveAsync();

    /// <summary>
    /// Gets all companies sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Company>> GetAllAsync();

    /// <summary>
    /// Searches companies by name and returns results sorted by name.
    /// </summary>
    Task<IEnumerable<Models.Company>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks whether a company exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Checks whether a company is active.
    /// </summary>
    Task<bool> IsActiveAsync(int id);
}
