using System.ComponentModel.DataAnnotations;
using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Exceptions;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.Extensions.Logging;
using ValidationException = EstateIQ.Exceptions.ValidationException;

namespace EstateIQ.Services;

/// <summary>
/// Provides business logic operations for property management.
/// </summary>
public class PropertyService(
    IPropertyRepository propertyRepository,
    IPropertyTypeRepository propertyTypeRepository,
    IPropertyStatusRepository propertyStatusRepository,
    ICompanyRepository companyRepository,
    IAgentRepository agentRepository,
    IAgentCompanyRepository agentCompanyRepository,
    IMapper mapper,
    ILogger<PropertyService> logger) : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository = propertyRepository;
    private readonly IPropertyTypeRepository _propertyTypeRepository = propertyTypeRepository;
    private readonly IPropertyStatusRepository _propertyStatusRepository = propertyStatusRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IAgentRepository _agentRepository = agentRepository;
    private readonly IAgentCompanyRepository _agentCompanyRepository = agentCompanyRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<PropertyService> _logger = logger;

    /// <summary>
    /// Gets all properties with related data.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> GetAllAsync()
    {
        var properties = await _propertyRepository.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<PropertyDto>>(properties);
    }

    /// <summary>
    /// Gets a single property by identifier with related data.
    /// </summary>
    public async Task<PropertyDto?> GetByIdAsync(int id)
    {
        var property = await _propertyRepository.GetByIdWithDetailsAsync(id);

        if (property is null)
        {
            _logger.LogWarning("Property {PropertyId} was not found.", id);
            throw new NotFoundException($"Property with id {id} was not found.");
        }

        _logger.LogInformation("Property {PropertyId} retrieved.", id);
        return _mapper.Map<PropertyDto>(property);
    }

    /// <summary>
    /// Creates a new property after validating input and business rules.
    /// </summary>
    public async Task<PropertyDto> CreateAsync(CreatePropertyDto dto)
    {
        try
        {
            _logger.LogInformation("Creating property with title {Title}.", dto.Title);

            ValidateDto(dto);
            await ValidatePropertyReferencesAsync(dto.PropertyTypeId, dto.PropertyStatusId, dto.CompanyId, dto.AgentId);
            await ValidateBusinessRulesForCreateAsync(dto.AgentId, dto.CompanyId);

            var property = _mapper.Map<Property>(dto);
            var created = await _propertyRepository.CreateAsync(property);
            var createdWithDetails = await _propertyRepository.GetByIdWithDetailsAsync(created.Id) ?? created;

            _logger.LogInformation("Property {PropertyId} created successfully.", created.Id);
            return _mapper.Map<PropertyDto>(createdWithDetails);
        }
        catch (ValidationException exception)
        {
            _logger.LogWarning(exception, "Property creation validation failed.");
            throw;
        }
        catch (BusinessRuleException exception)
        {
            _logger.LogWarning(exception, "Property creation business rule failed.");
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating property.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing property after validating input and business rules.
    /// </summary>
    public async Task<PropertyDto> UpdateAsync(int id, UpdatePropertyDto dto)
    {
        try
        {
            dto.Id = id;
            ValidateDto(dto);

            var existing = await _propertyRepository.GetByIdWithDetailsAsync(id);

            if (existing is null)
            {
                _logger.LogWarning("Attempted update for missing property {PropertyId}.", id);
                throw new NotFoundException($"Property with id {id} was not found.");
            }

            await ValidatePropertyReferencesAsync(dto.PropertyTypeId, dto.PropertyStatusId, dto.CompanyId, dto.AgentId);
            await ValidateBusinessRulesForUpdateAsync(existing, dto);

            var property = _mapper.Map<Property>(dto);
            var updated = await _propertyRepository.UpdateAsync(property);
            var updatedWithDetails = await _propertyRepository.GetByIdWithDetailsAsync(updated.Id) ?? updated;

            _logger.LogInformation("Property {PropertyId} updated successfully.", id);
            return _mapper.Map<PropertyDto>(updatedWithDetails);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException exception)
        {
            _logger.LogWarning(exception, "Property update validation failed for property {PropertyId}.", id);
            throw;
        }
        catch (BusinessRuleException exception)
        {
            _logger.LogWarning(exception, "Property update business rule failed for property {PropertyId}.", id);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating property {PropertyId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a property if business rules allow it.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var property = await _propertyRepository.GetByIdWithDetailsAsync(id);

            if (property is null)
            {
                _logger.LogWarning("Attempted delete for missing property {PropertyId}.", id);
                return false;
            }

            if (!CanDeleteProperty(property))
            {
                _logger.LogWarning("Attempt to delete protected property {PropertyId} with status {Status}.", id, property.PropertyStatus.Name);
                throw new BusinessRuleException("Sold or rented properties cannot be deleted.");
            }

            var deleted = await _propertyRepository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Audit: Property {PropertyId} deleted.", id);
            }

            return deleted;
        }
        catch (BusinessRuleException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting property {PropertyId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Gets properties filtered by city.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> GetByCityAsync(string city)
    {
        var properties = await _propertyRepository.GetByCityAsync(city);
        return await MapDetailedDtosAsync(properties.Select(x => x.Id));
    }

    /// <summary>
    /// Gets properties filtered by price range.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var properties = await _propertyRepository.GetByPriceRangeAsync(minPrice, maxPrice);
        return await MapDetailedDtosAsync(properties.Select(x => x.Id));
    }

    /// <summary>
    /// Gets properties filtered by property type.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> GetByPropertyTypeAsync(int propertyTypeId)
    {
        var properties = await _propertyRepository.GetByPropertyTypeAsync(propertyTypeId);
        return await MapDetailedDtosAsync(properties.Select(x => x.Id));
    }

    /// <summary>
    /// Gets properties filtered by status.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> GetByStatusAsync(int statusId)
    {
        var properties = await _propertyRepository.GetByStatusAsync(statusId);
        return await MapDetailedDtosAsync(properties.Select(x => x.Id));
    }

    /// <summary>
    /// Searches properties by keyword.
    /// </summary>
    public async Task<IEnumerable<PropertyDto>> SearchAsync(string keyword)
    {
        var properties = await _propertyRepository.SearchAsync(keyword);
        return await MapDetailedDtosAsync(properties.Select(x => x.Id));
    }

    /// <summary>
    /// Gets a paginated list of properties with sorting.
    /// </summary>
    public async Task<PagedResult<PropertyDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true)
    {
        var (items, totalCount) = await _propertyRepository.GetPagedAsync(pageNumber, pageSize, sortBy, ascending);
        var detailedItems = await MapDetailedDtosAsync(items.Select(x => x.Id));

        return new PagedResult<PropertyDto>
        {
            Items = detailedItems.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Changes the status of a property.
    /// </summary>
    public async Task<PropertyDto> ChangeStatusAsync(int id, int newStatusId)
    {
        try
        {
            if (!await _propertyStatusRepository.ExistsAsync(newStatusId))
            {
                throw BuildValidationException(nameof(newStatusId), $"Property status with id {newStatusId} does not exist.");
            }

            var existing = await _propertyRepository.GetByIdWithDetailsAsync(id);

            if (existing is null)
            {
                throw new NotFoundException($"Property with id {id} was not found.");
            }

            var updateDto = MapToUpdateDto(existing);
            updateDto.PropertyStatusId = newStatusId;

            return await UpdateAsync(id, updateDto);
        }
        catch (Exception exception) when (exception is not NotFoundException and not ValidationException and not BusinessRuleException)
        {
            _logger.LogError(exception, "Unexpected error while changing status for property {PropertyId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Updates the price of a property.
    /// </summary>
    public async Task<PropertyDto> UpdatePriceAsync(int id, decimal newPrice)
    {
        try
        {
            if (newPrice <= 0)
            {
                throw BuildValidationException(nameof(newPrice), "Price must be greater than zero.");
            }

            var existing = await _propertyRepository.GetByIdWithDetailsAsync(id);

            if (existing is null)
            {
                throw new NotFoundException($"Property with id {id} was not found.");
            }

            if (IsStatus(existing, SoldStatusName))
            {
                _logger.LogWarning("Attempt to change price for sold property {PropertyId}.", id);
                throw new BusinessRuleException("Sold properties cannot have their price changed.");
            }

            var updateDto = MapToUpdateDto(existing);
            updateDto.Price = newPrice;

            return await UpdateAsync(id, updateDto);
        }
        catch (Exception exception) when (exception is not NotFoundException and not ValidationException and not BusinessRuleException)
        {
            _logger.LogError(exception, "Unexpected error while updating price for property {PropertyId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Evaluates whether a property can be deleted.
    /// </summary>
    public async Task<bool> CanDeleteAsync(int id)
    {
        var property = await _propertyRepository.GetByIdWithDetailsAsync(id);
        return property is not null && CanDeleteProperty(property);
    }

    private const string SoldStatusName = "Sold";
    private const string RentedStatusName = "Rented";

    private async Task<IEnumerable<PropertyDto>> MapDetailedDtosAsync(IEnumerable<int> propertyIds)
    {
        var orderedIds = propertyIds.Distinct().ToList();

        if (orderedIds.Count == 0)
        {
            return [];
        }

        var properties = await _propertyRepository.GetAllWithDetailsAsync();
        var propertyLookup = properties.ToDictionary(x => x.Id);

        var orderedProperties = orderedIds
            .Where(propertyLookup.ContainsKey)
            .Select(id => propertyLookup[id])
            .ToList();

        return _mapper.Map<IEnumerable<PropertyDto>>(orderedProperties);
    }

    private static void ValidateDto(object dto)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        Validator.TryValidateObject(dto, validationContext, validationResults, true);

        var errors = validationResults
            .SelectMany(result =>
            {
                var members = result.MemberNames.Any()
                    ? result.MemberNames
                    : [string.Empty];

                return members.Select(member => new
                {
                    Member = member,
                    Error = result.ErrorMessage ?? "Validation error."
                });
            })
            .GroupBy(x => x.Member)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Error).Distinct().ToArray());

        switch (dto)
        {
            case CreatePropertyDto createPropertyDto:
                AddFutureYearError(createPropertyDto.YearBuilt, errors, nameof(CreatePropertyDto.YearBuilt));
                break;
            case UpdatePropertyDto updatePropertyDto:
                AddFutureYearError(updatePropertyDto.YearBuilt, errors, nameof(UpdatePropertyDto.YearBuilt));
                break;
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void AddFutureYearError(int? yearBuilt, IDictionary<string, string[]> errors, string propertyName)
    {
        if (yearBuilt is null || yearBuilt <= DateTime.UtcNow.Year)
        {
            return;
        }

        errors.TryGetValue(propertyName, out var existingErrors);
        errors[propertyName] = [.. existingErrors ?? [], "YearBuilt cannot be in the future."];
    }

    private async Task ValidatePropertyReferencesAsync(int propertyTypeId, int propertyStatusId, int companyId, int agentId)
    {
        var errors = new Dictionary<string, string[]>();

        if (!await _propertyTypeRepository.ExistsAsync(propertyTypeId))
        {
            errors[nameof(CreatePropertyDto.PropertyTypeId)] = [$"Property type with id {propertyTypeId} does not exist."];
        }

        if (!await _propertyStatusRepository.ExistsAsync(propertyStatusId))
        {
            errors[nameof(CreatePropertyDto.PropertyStatusId)] = [$"Property status with id {propertyStatusId} does not exist."];
        }

        if (!await _companyRepository.ExistsAsync(companyId))
        {
            errors[nameof(CreatePropertyDto.CompanyId)] = [$"Company with id {companyId} does not exist."];
        }
        else if (!await _companyRepository.IsActiveAsync(companyId))
        {
            errors[nameof(CreatePropertyDto.CompanyId)] = [$"Company with id {companyId} is not active."];
        }

        if (!await _agentRepository.ExistsAsync(agentId))
        {
            errors[nameof(CreatePropertyDto.AgentId)] = [$"Agent with id {agentId} does not exist."];
        }
        else if (!await _agentRepository.IsActiveAsync(agentId))
        {
            errors[nameof(CreatePropertyDto.AgentId)] = [$"Agent with id {agentId} is not active."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private async Task ValidateBusinessRulesForCreateAsync(int agentId, int companyId)
    {
        if (!await _agentCompanyRepository.ExistsActiveRelationshipAsync(agentId, companyId))
        {
            _logger.LogWarning("Agent {AgentId} is not associated with company {CompanyId}.", agentId, companyId);
            throw new BusinessRuleException("Agent is not associated with this company.");
        }
    }

    private async Task ValidateBusinessRulesForUpdateAsync(Property existing, UpdatePropertyDto dto)
    {
        await ValidateBusinessRulesForCreateAsync(dto.AgentId, dto.CompanyId);

        if (IsStatus(existing, SoldStatusName) && existing.Price != dto.Price)
        {
            _logger.LogWarning("Attempt to change price for sold property {PropertyId}.", existing.Id);
            throw new BusinessRuleException("Sold properties cannot have their price changed.");
        }

        if (IsStatus(existing, RentedStatusName) && HasRentedReadOnlyChanges(existing, dto))
        {
            _logger.LogWarning("Attempt to modify read-only fields for rented property {PropertyId}.", existing.Id);
            throw new BusinessRuleException("Rented properties have read-only structural and assignment fields.");
        }
    }

    private static bool HasRentedReadOnlyChanges(Property existing, UpdatePropertyDto dto)
    {
        return existing.Area != dto.Area ||
               existing.Bedrooms != dto.Bedrooms ||
               existing.Bathrooms != dto.Bathrooms ||
               existing.Floors != dto.Floors ||
               existing.YearBuilt != dto.YearBuilt ||
               existing.PropertyTypeId != dto.PropertyTypeId ||
               existing.CompanyId != dto.CompanyId ||
               existing.AgentId != dto.AgentId ||
               !string.Equals(existing.Address, dto.Address, StringComparison.Ordinal) ||
               !string.Equals(existing.City, dto.City, StringComparison.Ordinal) ||
               existing.Latitude != dto.Latitude ||
               existing.Longitude != dto.Longitude;
    }

    private static bool CanDeleteProperty(Property property)
    {
        return !IsStatus(property, SoldStatusName) && !IsStatus(property, RentedStatusName);
    }

    private static bool IsStatus(Property property, string statusName)
    {
        return string.Equals(property.PropertyStatus?.Name, statusName, StringComparison.OrdinalIgnoreCase);
    }

    private static UpdatePropertyDto MapToUpdateDto(Property property)
    {
        return new UpdatePropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Area = property.Area,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Floors = property.Floors,
            YearBuilt = property.YearBuilt,
            PropertyTypeId = property.PropertyTypeId,
            PropertyStatusId = property.PropertyStatusId,
            CompanyId = property.CompanyId,
            AgentId = property.AgentId,
            Address = property.Address,
            City = property.City,
            Latitude = property.Latitude,
            Longitude = property.Longitude
        };
    }

    private static ValidationException BuildValidationException(string key, string message)
    {
        return new ValidationException(new Dictionary<string, string[]>
        {
            [key] = [message]
        });
    }
}
