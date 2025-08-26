using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using Healink.Application.Common.Interfaces.Repositories;

namespace Healink.Application.Features.Staff.Commands.UpdateProfile;

/// <summary>
/// Handler for update profile command
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<StaffProfileDto>>
{
    private readonly IStaffProfileRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(
        IStaffProfileRepository staffRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateProfileCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StaffProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to update staff profile: {StaffId}", request.StaffId);

            // Get the existing staff profile
            var existingStaff = await _staffRepository.GetByIdAsync(request.StaffId);
            if (existingStaff == null)
            {
                _logger.LogWarning("Staff profile not found: {StaffId}", request.StaffId);
                return Result<StaffProfileDto>.Failure("Staff profile not found", ErrorCode.NotFound, 404);
            }

            // Check if current user can update this profile
            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsInRole("Admin");
            
            if (!isAdmin && existingStaff.AppUserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to update profile {StaffId} without permission", 
                    currentUserId, request.StaffId);
                return Result<StaffProfileDto>.Failure("You don't have permission to update this profile", ErrorCode.Forbidden, 403);
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.FirstName))
                existingStaff.FirstName = request.FirstName.Trim();
            
            if (!string.IsNullOrEmpty(request.LastName))
                existingStaff.LastName = request.LastName.Trim();
            
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                existingStaff.PhoneNumber = request.PhoneNumber.Trim();
            
            if (request.DateOfBirth.HasValue)
                existingStaff.DateOfBirth = request.DateOfBirth.Value;
            
            if (request.Gender.HasValue)
                existingStaff.Gender = request.Gender.Value;
            
            if (!string.IsNullOrEmpty(request.Address))
                existingStaff.Address = request.Address.Trim();
            
            if (!string.IsNullOrEmpty(request.Department))
                existingStaff.Department = request.Department.Trim();
            
            if (!string.IsNullOrEmpty(request.Position))
                existingStaff.Position = request.Position.Trim();

            // Update the entity
            existingStaff.UpdatedAt = DateTime.UtcNow;
            _staffRepository.Update(existingStaff);
            
            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var result = _mapper.Map<StaffProfileDto>(existingStaff);
            
            _logger.LogInformation("Staff profile updated successfully: {StaffId}", request.StaffId);
            
            return Result<StaffProfileDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating staff profile: {StaffId}", request.StaffId);
            return Result<StaffProfileDto>.Failure("An error occurred while updating the profile", ErrorCode.InternalServerError);
        }
    }
}
