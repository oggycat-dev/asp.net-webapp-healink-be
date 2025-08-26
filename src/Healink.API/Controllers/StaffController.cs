using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Features.Staff.Commands.CreateStaff;
using Healink.Application.Features.Staff.Commands.UpdateProfile;
using Healink.Application.Features.Staff.Queries.GetStaff;
using Healink.API.Middlewares;
using Healink.API.Extensions;
using Healink.Domain.Enums;

namespace Healink.API.Controllers;

/// <summary>
/// Staff management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMediator mediator, ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new staff member
    /// </summary>
    /// <param name="request">Staff creation request</param>
    /// <returns>Created staff response</returns>
    [HttpPost]
    [RequireAdminRole]
    [ProducesResponseType(typeof(CreatedStaffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        try
        {
            _logger.LogInformation("Creating staff member with email: {Email}", request.Email);

            var command = new CreateStaffCommand
            {
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender ?? Gender.Other,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                Role = request.Role,
                Department = request.Department,
                LicenseNumber = request.LicenseNumber,
                EmergencyContact = request.EmergencyContact
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Staff creation failed for email: {Email}. Reason: {Error}", 
                    request.Email, result.Message);
                
                return this.FromResult(result);
            }

            _logger.LogInformation("Staff member created successfully: {StaffId}", result.Data!.Id);
            return this.Created(result.Data, "Staff member created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member with email: {Email}", request.Email);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get staff member by ID
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <returns>Staff information</returns>
    [HttpGet("{id:guid}")]
    [RequireStaffRole]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStaff(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving staff member: {StaffId}", id);

            var query = new GetStaffQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Staff retrieval failed for ID: {StaffId}. Reason: {Error}", 
                    id, result.Message);
                
                return this.FromResult(result);
            }

            _logger.LogInformation("Staff member retrieved successfully: {StaffId}", id);
            return this.Success(result.Data, "Staff information retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff member: {StaffId}", id);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get current staff profile
    /// </summary>
    /// <returns>Current staff information</returns>
    [HttpGet("me")]
    [RequireStaffRole]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var entityIdClaim = User.FindFirst("entity_id")?.Value;
            
            if (string.IsNullOrEmpty(entityIdClaim) || !Guid.TryParse(entityIdClaim, out var entityId))
            {
                _logger.LogWarning("Invalid or missing entity ID in user claims");
                return BadRequest(new { message = "Invalid user session" });
            }

            _logger.LogInformation("Retrieving current staff profile: {StaffId}", entityId);

            var query = new GetStaffQuery(entityId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Current staff profile retrieval failed. Reason: {Error}", result.Message);
                
                return this.FromResult(result);
            }

            _logger.LogInformation("Current staff profile retrieved successfully");
            return this.Success(result.Data, "Current user profile retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current staff profile");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Update staff profile
    /// </summary>
    /// <param name="id">Staff ID to update</param>
    /// <param name="request">Update profile request</param>
    /// <returns>Updated staff profile</returns>
    [HttpPut("{id:guid}/profile")]
    [RequireStaffRole]
    [ProducesResponseType(typeof(StaffProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateStaffProfileDto request)
    {
        try
        {
            _logger.LogInformation("Updating staff profile: {StaffId}", id);

            var command = new UpdateProfileCommand
            {
                StaffId = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                Department = request.Department,
                Position = request.Position
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Staff profile update failed for ID: {StaffId}. Reason: {Error}", 
                    id, result.Message);
                
                return this.FromResult(result);
            }

            _logger.LogInformation("Staff profile updated successfully: {StaffId}", id);
            return this.Success(result.Data, "Staff profile updated successfully");
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for staff profile update: {StaffId}. Errors: {Errors}", 
                id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            
            return this.ValidationError(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff profile: {StaffId}", id);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Update current user's staff profile
    /// </summary>
    /// <param name="request">Update profile request</param>
    /// <returns>Updated staff profile</returns>
    [HttpPut("me/profile")]
    [RequireStaffRole]
    [ProducesResponseType(typeof(StaffProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStaffProfileDto request)
    {
        try
        {
            var entityIdClaim = User.FindFirst("entity_id")?.Value;
            
            if (string.IsNullOrEmpty(entityIdClaim) || !Guid.TryParse(entityIdClaim, out var entityId))
            {
                _logger.LogWarning("Invalid or missing entity ID in user claims");
                return BadRequest(new { message = "Invalid user session" });
            }

            _logger.LogInformation("Updating current user's staff profile: {StaffId}", entityId);

            var command = new UpdateProfileCommand
            {
                StaffId = entityId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                Department = request.Department,
                Position = request.Position
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Current user's staff profile update failed. Reason: {Error}", result.Message);
                
                return this.FromResult(result);
            }

            _logger.LogInformation("Current user's staff profile updated successfully");
            return this.Success(result.Data, "Your profile updated successfully");
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for current user's staff profile update. Errors: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            
            return this.ValidationError(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user's staff profile");
            return this.InternalServerError();
        }
    }
} 