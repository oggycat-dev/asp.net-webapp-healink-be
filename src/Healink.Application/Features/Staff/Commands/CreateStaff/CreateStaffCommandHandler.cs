using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Features.Staff.Commands.CreateStaff;

/// <summary>
/// Handler for creating staff command
/// </summary>
public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Result<CreatedStaffResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateStaffCommandHandler> _logger;

    public CreateStaffCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateStaffCommandHandler> logger)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<CreatedStaffResponse>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new staff member: {Email}", request.Email);

            // Validate uniqueness
            var emailUnique = await _identityService.IsEmailUniqueAsync(request.Email);
            if (!emailUnique.IsSuccess || !emailUnique.Data)
            {
                return Result<CreatedStaffResponse>.Failure("Email already exists", ErrorCode.ValidationFailed);
            }

            var usernameUnique = await _identityService.IsUsernameUniqueAsync(request.Username);
            if (!usernameUnique.IsSuccess || !usernameUnique.Data)
            {
                return Result<CreatedStaffResponse>.Failure("Username already exists", ErrorCode.ValidationFailed);
            }

            var phoneUnique = await _identityService.IsPhoneUniqueAsync(request.PhoneNumber);
            if (!phoneUnique.IsSuccess || !phoneUnique.Data)
            {
                return Result<CreatedStaffResponse>.Failure("Phone number already exists", ErrorCode.ValidationFailed);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create identity account
                var registrationRequest = new StaffRegistrationRequest
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Department = request.Department
                };

                var identityResult = await _identityService.RegisterStaffAsync(registrationRequest);
                if (!identityResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<CreatedStaffResponse>.Failure(identityResult.Message ?? "Staff registration failed", ErrorCode.ValidationFailed);
                }

                var identityResponse = identityResult.Data!;

                // Create staff profile
                var staffProfile = new StaffProfile
                {
                    Id = identityResponse.EntityId,
                    IdentityUserId = identityResponse.UserId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FullName = $"{request.FirstName} {request.LastName}",
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    Department = request.Department,
                    LicenseNumber = request.LicenseNumber,
                    EmergencyContact = request.EmergencyContact,
                    JoinDate = DateTime.UtcNow,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse(_currentUserService.UserId ?? Guid.NewGuid().ToString())
                };

                await _unitOfWork.StaffProfileRepository.AddAsync(staffProfile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var response = new CreatedStaffResponse
                {
                    Id = staffProfile.Id,
                    UserId = identityResponse.UserId,
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FullName = staffProfile.FullName,
                    Role = request.Role,
                    Department = request.Department,
                    Status = staffProfile.Status,
                    CreatedAt = staffProfile.CreatedAt,
                    IsSuccess = true,
                    Message = "Staff member created successfully"
                };

                _logger.LogInformation("Staff member created successfully: {StaffId} - {Email}", staffProfile.Id, request.Email);

                return Result<CreatedStaffResponse>.Success(response);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating staff member: {Email}", request.Email);
            return Result<CreatedStaffResponse>.Failure("An error occurred while creating staff member", ErrorCode.InternalServerError);
        }
    }
} 