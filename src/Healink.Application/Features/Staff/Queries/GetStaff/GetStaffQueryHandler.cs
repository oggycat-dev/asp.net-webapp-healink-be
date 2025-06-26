using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Staff.Queries.GetStaff;

/// <summary>
/// Handler for getting staff query
/// </summary>
public class GetStaffQueryHandler : IRequestHandler<GetStaffQuery, Result<StaffResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStaffQueryHandler> _logger;

    public GetStaffQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetStaffQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StaffResponse>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving staff member: {StaffId}", request.StaffId);

            var staffProfile = await _unitOfWork.StaffProfileRepository.GetByIdAsync(request.StaffId);

            if (staffProfile == null)
            {
                _logger.LogWarning("Staff member not found: {StaffId}", request.StaffId);
                return Result<StaffResponse>.Failure("Staff member not found", ErrorCode.NotFound);
            }

            var response = _mapper.Map<StaffResponse>(staffProfile);

            _logger.LogInformation("Staff member retrieved successfully: {StaffId}", request.StaffId);

            return Result<StaffResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving staff member: {StaffId}", request.StaffId);
            return Result<StaffResponse>.Failure("An error occurred while retrieving staff member", ErrorCode.InternalServerError);
        }
    }
} 