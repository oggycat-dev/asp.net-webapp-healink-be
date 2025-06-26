using AutoMapper;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using MediatR;

namespace Healink.Application.Features.Staff.Queries.GetStaffProfiles;

/// <summary>
/// Handler for getting staff profiles with filtering and pagination
/// </summary>
public class GetStaffProfilesQueryHandler : IRequestHandler<GetStaffProfilesQuery, Result<List<StaffProfileDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStaffProfilesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<StaffProfileDto>>> Handle(GetStaffProfilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var staffProfiles = await _unitOfWork.StaffProfileRepository.GetFilteredAsync(
                searchTerm: request.SearchTerm,
                department: request.Department,
                position: request.Position,
                status: request.Status,
                sortBy: request.SortBy,
                isAscending: request.IsAscending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var staffDtos = _mapper.Map<List<StaffProfileDto>>(staffProfiles);
            
            return Result<List<StaffProfileDto>>.Success(staffDtos);
        }
        catch (Exception ex)
        {
            return Result<List<StaffProfileDto>>.Failure($"An error occurred while retrieving staff profiles: {ex.Message}");
        }
    }
} 