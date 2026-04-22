namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for companies.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Checks whether a company exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Checks whether a company is active.
    /// </summary>
    Task<bool> IsActiveAsync(int id);
}
