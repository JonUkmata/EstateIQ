using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using EstateIQ.Models;

namespace EstateIQ.Services;

/// <summary>
/// Provides business logic operations for property type lookups.
/// </summary>
public class PropertyTypeService(
    IPropertyTypeRepository propertyTypeRepository,
    IMapper mapper,
    ILogger<PropertyTypeService> logger) : IPropertyTypeService
{
    private readonly IPropertyTypeRepository _propertyTypeRepository = propertyTypeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<PropertyTypeService> _logger = logger;

    /// <summary>
    /// Gets property types for dropdown usage.
    /// </summary>
    public async Task<IEnumerable<PropertyTypeDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        _logger.LogInformation(
            "Fetching property types for dropdown. IncludeInactive: {IncludeInactive}, Search: {Search}",
            includeInactive,
            normalizedSearch ?? "None");

        IEnumerable<PropertyType> propertyTypes;

        if (normalizedSearch is not null)
        {
            propertyTypes = await _propertyTypeRepository.SearchByNameAsync(normalizedSearch);

            if (!includeInactive)
            {
                propertyTypes = propertyTypes.Where(propertyType => propertyType.IsActive);
            }
        }
        else if (includeInactive)
        {
            propertyTypes = await _propertyTypeRepository.GetAllAsync();
        }
        else
        {
            propertyTypes = await _propertyTypeRepository.GetAllActiveAsync();
        }

        var result = _mapper.Map<IEnumerable<PropertyTypeDto>>(propertyTypes).ToList();
        _logger.LogInformation("Returned {PropertyTypeCount} property types for dropdown.", result.Count);

        return result;
    }
}
