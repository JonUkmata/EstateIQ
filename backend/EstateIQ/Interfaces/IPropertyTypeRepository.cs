namespace EstateIQ.Interfaces;

/// <summary>
/// Provides lookup operations for property types.
/// </summary>
public interface IPropertyTypeRepository
{
    /// <summary>
    /// Checks whether a property type exists.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
