using AutoMapper;
using Healink.Application.Common.DTOs.Staff;
using Healink.Domain.Entities;

namespace Healink.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for Staff entities
/// </summary>
public class StaffMappingProfile : Profile
{
    public StaffMappingProfile()
    {
        // StaffProfile to StaffResponse mapping
        CreateMap<StaffProfile, StaffResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ReverseMap();

        // CreateStaffRequest to StaffProfile mapping
        CreateMap<CreateStaffRequest, StaffProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
            .ForMember(dest => dest.IdentityUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.EntityStatus.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ReverseMap();

        // StaffProfile to CreatedStaffResponse mapping
        CreateMap<StaffProfile, CreatedStaffResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ReverseMap();
    }
} 