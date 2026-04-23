using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Interfaces;
using EstateIQ.Models;

namespace EstateIQ.Services;

/// <summary>
/// Provides business logic operations for company lookups.
/// </summary>
public class CompanyService(
    ICompanyRepository companyRepository,
    IMapper mapper,
    ILogger<CompanyService> logger) : ICompanyService
{
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CompanyService> _logger = logger;

    /// <summary>
    /// Gets companies for dropdown usage.
    /// </summary>
    public async Task<IEnumerable<CompanyDropdownDto>> GetForDropdownAsync(
        bool includeInactive = false,
        string? search = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        _logger.LogInformation(
            "Fetching companies for dropdown. IncludeInactive: {IncludeInactive}, Search: {Search}",
            includeInactive,
            normalizedSearch ?? "None");

        IEnumerable<Company> companies;

        if (normalizedSearch is not null)
        {
            companies = await _companyRepository.SearchByNameAsync(normalizedSearch);

            if (!includeInactive)
            {
                companies = companies.Where(company => company.IsActive);
            }
        }
        else if (includeInactive)
        {
            companies = await _companyRepository.GetAllAsync();
        }
        else
        {
            companies = await _companyRepository.GetAllActiveAsync();
        }

        var result = _mapper.Map<IEnumerable<CompanyDropdownDto>>(companies).ToList();
        _logger.LogInformation("Returned {CompanyCount} companies for dropdown.", result.Count);

        return result;
    }
}
