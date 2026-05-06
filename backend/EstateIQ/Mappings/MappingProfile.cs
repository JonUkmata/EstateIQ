using AutoMapper;
using EstateIQ.DTOs;
using EstateIQ.Models;

namespace EstateIQ.Mappings;

/// <summary>
/// Defines AutoMapper mappings for API DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Property, PropertyDto>()
            .ForMember(destination => destination.Images, options => options.Ignore());
        CreateMap<CreatePropertyDto, Property>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.PropertyType, options => options.Ignore())
            .ForMember(destination => destination.PropertyStatus, options => options.Ignore())
            .ForMember(destination => destination.Company, options => options.Ignore())
            .ForMember(destination => destination.Agent, options => options.Ignore());

        CreateMap<UpdatePropertyDto, Property>()
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.PropertyType, options => options.Ignore())
            .ForMember(destination => destination.PropertyStatus, options => options.Ignore())
            .ForMember(destination => destination.Company, options => options.Ignore())
            .ForMember(destination => destination.Agent, options => options.Ignore());

        CreateMap<PropertyType, PropertyTypeDto>();
        CreateMap<PropertyStatus, PropertyStatusDto>();
        CreateMap<Company, CompanyDto>();
        CreateMap<Company, CompanyDropdownDto>();
        CreateMap<Agent, AgentDto>();
    }
}
