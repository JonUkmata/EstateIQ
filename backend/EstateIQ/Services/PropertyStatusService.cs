using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using EstateIQ.Models;

namespace EstateIQ.Services;

/// <summary>
/// Provides business logic operations for property status lookups.
/// </summary>
public class PropertyStatusService(
    IPropertyStatusRepository propertyStatusRepository,
    IMapper mapper,
    ILogger<PropertyStatusService> logger) : IPropertyStatusService
{
    private readonly IPropertyStatusRepository _propertyStatusRepository = propertyStatusRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<PropertyStatusService> _logger = logger;

    /// <summary>
    /// Gets property statuses for dropdown usage.
    /// </summary>
    public async Task<IEnumerable<PropertyStatusDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        _logger.LogInformation(
            "Fetching property statuses for dropdown. IncludeInactive: {IncludeInactive}, Search: {Search}",
            includeInactive,
            normalizedSearch ?? "None");

        IEnumerable<PropertyStatus> statuses;

        if (normalizedSearch is not null)
        {
            statuses = await _propertyStatusRepository.SearchByNameAsync(normalizedSearch);

            if (!includeInactive)
            {
                statuses = statuses.Where(status => status.IsActive);
            }
        }
        else if (includeInactive)
        {
            statuses = await _propertyStatusRepository.GetAllAsync();
        }
        else
        {
            statuses = await _propertyStatusRepository.GetAllActiveAsync();
        }

        var result = _mapper.Map<IEnumerable<PropertyStatusDto>>(statuses).ToList();
        _logger.LogInformation("Returned {StatusCount} property statuses for dropdown.", result.Count);

        return result;
    }
}
