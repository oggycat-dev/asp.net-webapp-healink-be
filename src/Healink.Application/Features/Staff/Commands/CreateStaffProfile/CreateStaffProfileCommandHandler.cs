using AutoMapper;
using FluentValidation;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Application.Common.Models;
using Healink.Domain.Entities;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Healink.Application.Features.Staff.Commands.CreateStaffProfile;

/// <summary>
/// Handler for creating staff profile
/// </summary>
public class CreateStaffProfileCommandHandler : IRequestHandler<CreateStaffProfileCommand, Result<StaffProfileDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStaffProfileCommandHandler> _logger;

    public CreateStaffProfileCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateStaffProfileCommandHandler> logger)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StaffProfileDto>> Handle(CreateStaffProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Request;
            _logger.LogInformation("Creating staff profile for {Email}", dto.Email);

            // Check if user already exists
            var existingUser = await _identityService.FindByEmailAsync(dto.Email);
            if (existingUser.IsSuccess)
            {
                _logger.LogWarning("User with email {Email} already exists", dto.Email);
                return Result<StaffProfileDto>.Failure("User with this email already exists", ErrorCode.UserAlreadyExists);
            }

            // Check phone number uniqueness
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var existingStaff = await _unitOfWork.StaffProfileRepository.FindAsync(
                    s => s.PhoneNumber == dto.PhoneNumber, cancellationToken);
                if (existingStaff.Any())
                {
                    _logger.LogWarning("Staff with phone number {PhoneNumber} already exists", dto.PhoneNumber);
                    return Result<StaffProfileDto>.Failure("Staff with this phone number already exists", ErrorCode.UserAlreadyExists);
                }
            }

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create AppUser for Identity
                var appUser = new AppUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                // Register the user
                var registrationResult = await _identityService.RegisterAsync(appUser, dto.Password);
                if (!registrationResult.IsSuccess)
                {
                    _logger.LogError("Failed to create identity user: {Error}", registrationResult.Message);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<StaffProfileDto>.Failure(
                        registrationResult.Message, 
                        ErrorCode.ValidationFailed);
                }

                // Add to Staff role
                await _identityService.AddToRoleAsync(registrationResult.Data!, "Staff");

                // Create staff profile using mapper
                var staffProfile = _mapper.Map<StaffProfile>(dto);
                staffProfile.AppUserId = registrationResult.Data!.Id;
                staffProfile.Status = EntityStatus.Active;

                await _unitOfWork.StaffProfileRepository.AddAsync(staffProfile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Successfully created staff profile for {Email} with ID {StaffId}", 
                    dto.Email, staffProfile.Id);

                // Map to response DTO
                var responseDto = _mapper.Map<StaffProfileDto>(staffProfile);

                return Result<StaffProfileDto>.Success(responseDto);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff profile for {Email}", request.Request.Email);
            return Result<StaffProfileDto>.Failure(
                "An error occurred while creating staff profile", 
                ErrorCode.InternalServerError);
        }
    }
} 